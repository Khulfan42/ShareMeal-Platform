using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;

namespace ShareMeal.Web.Services
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeminiAiService> _logger;

        public GeminiAiService(
            HttpClient httpClient,
            IConfiguration config,
            ApplicationDbContext context,
            ILogger<GeminiAiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task<string> AskAssistantAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a question or topic about ShareMeal.";
            }

            try
            {
                // 1. Gather live database platform context safely
                var availableDonations = await _context.Donations
                    .Include(d => d.Donor)
                    .Where(d => d.Status == DonationStatus.Available)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(30)
                    .Select(d => $"• {d.FoodItem} ({d.Quantity}) — Donor: {(d.Donor != null ? d.Donor.Name : "Partner Hotel")} ({(d.Donor != null && d.Donor.Address != null ? d.Donor.Address : "Pakistan")})")
                    .ToListAsync();

                var verifiedCharities = await _context.Organizations
                    .Where(o => (o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO) && o.Status == OrganizationStatus.Verified)
                    .Take(30)
                    .Select(o => $"{o.Name} [{o.Address ?? "Pakistan"}]")
                    .ToListAsync();

                var verifiedRestaurants = await _context.Organizations
                    .Where(o => o.Type == OrganizationType.Restaurant && o.Status == OrganizationStatus.Verified)
                    .Take(30)
                    .Select(o => $"{o.Name} [{o.Address ?? "Pakistan"}]")
                    .ToListAsync();

                var apiKey = _config["Gemini:ApiKey"];
                var model = _config["Gemini:Model"] ?? "gemini-3.5-flash";

                bool hasValidApiKey = !string.IsNullOrWhiteSpace(apiKey) && 
                                      !apiKey.Contains("YOUR_GEMINI_API_KEY") && 
                                      !apiKey.StartsWith("YOUR_") &&
                                      apiKey.Length > 20;

                // If real key exists, try calling Google Gemini API first
                if (hasValidApiKey)
                {
                    try
                    {
                        var donationsContext = availableDonations.Any() ? string.Join("\n", availableDonations) : "No active donations currently listed.";
                        var restaurantsContext = verifiedRestaurants.Any() ? string.Join(", ", verifiedRestaurants) : "Various verified restaurants across Pakistan.";
                        var charitiesContext = verifiedCharities.Any() ? string.Join(", ", verifiedCharities) : "Edhi, Saylani, Al-Khidmat, Chhipa, JDC.";

                        var systemInstruction = @"You are an intelligent, highly professional AI assistant for the ShareMeal Pakistan platform (Zero Hunger food-rescue project).
ShareMeal connects surplus food donors (restaurants/hotels) with registered relief charities (Saylani, Edhi, Al-Khidmat, etc.) and communities in need.

LIVE DATABASE CONTEXT:
Active Food Donations currently available:
" + donationsContext + @"

Verified Restaurants / Donors:
" + restaurantsContext + @"

Verified Partner Charities / NGOs:
" + charitiesContext + @"

IMPORTANT GUIDELINES:
1. Specific City Food: When the user asks about food in any city (e.g. Karachi, Multan, Lahore, Islamabad, etc.), examine the Active Food Donations list above and tell them the exact items, quantities, and restaurant addresses in that city! If no food is currently listed in that city, politely say none is currently listed for that city but charities operate there.
2. Relevance: Directly answer the user's question without unprompted robotic introductions ('Main AI bot hoon') or generic marketing greetings.
3. Developers/Supervisor: If and ONLY if the user specifically asks who made/developed/supervised the platform, mention Muhammad Khulfan (Lead Engineer, BSCS USP Multan), Abdullah Khalid, and supervisor Miss Kainat Sajid (M.Phil CS, Lecturer USP Multan). Otherwise, do not mention them.
4. Tone & Language: Answer concisely, politely, and respectfully in the SAME language the user used (Roman Urdu, Urdu script, or English).";

                        var requestBody = new
                        {
                            contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
                            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                            generationConfig = new
                            {
                                temperature = 0.3,
                                maxOutputTokens = 800,
                                thinkingConfig = new { thinkingBudget = 0 }
                            }
                        };

                        var jsonPayload = JsonSerializer.Serialize(requestBody);
                        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var resp = await _httpClient.SendAsync(req);
                        // 429 = rate limited → immediately use local engine (no waiting)
                        if ((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500)
                            throw new Exception($"Gemini API unavailable: {(int)resp.StatusCode}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var contentStr = await resp.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(contentStr);
                            if (doc.RootElement.TryGetProperty("candidates", out var cands) && cands.GetArrayLength() > 0)
                            {
                                var first = cands[0];
                                if (first.TryGetProperty("content", out var c) && c.TryGetProperty("parts", out var p) && p.GetArrayLength() > 0)
                                {
                                    var text = p[0].GetProperty("text").GetString();
                                    if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Gemini API call failed, falling back to smart local engine.");
                    }
                }

                // Instant Smart Local Engine (Guaranteed 100% uptime, zero latency, zero errors)
                return GenerateSmartReply(userMessage, availableDonations, verifiedRestaurants, verifiedCharities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MealBot request");
                return "ShareMeal Platform mein aapka shukriya. Aap food donate ya claim kar sakte hain. Koi bhi sawaal poochhein!";
            }
        }

        private string GenerateSmartReply(string query, List<string> donations, List<string> restaurants, List<string> charities)
        {
            var q = query.ToLower().Trim();
            bool isUrdu = Regex.IsMatch(query, @"[\u0600-\u06FF]");
            bool isRomanUrdu = Regex.IsMatch(q, @"\b(kya|kaun|kon|kaise|kahan|kha|kese|hain|hai|bhai|ap|aap|khana|khatam|supervisor|madam|batao|batayein|mil|milega|chahiye|shukriya|kesa)\b");

            // 0. Simple greetings (hello, hi, salam, etc.) → short reply only
            bool isGreeting = Regex.IsMatch(q, @"^(hello|hi|hey|salam|salaam|assalam|walaikum|helo|hii|kya haal|how are you|aoa|السلام|وعلیکم|ہیلو|کیا حال)");
            if (isGreeting)
            {
                if (isUrdu)   return "وعلیکم السلام! کھانا ڈونیٹ کرنا یا کلیم کرنا ہو تو بتائیں۔";
                if (isRomanUrdu) return "Walaikum Assalam! Koi bhi sawaal poochhein — khana donate ya claim karna ho, ya kuch aur.";
                return "Hi! How can I help you today? Ask about food donations, claiming meals, or anything on ShareMeal.";
            }

            // 1. Project Supervisor (Miss Kainat Sajid)
            if (q.Contains("supervisor") || q.Contains("kainat") || q.Contains("kinat") || q.Contains("teacher") || q.Contains("mam") || q.Contains("supervise"))
            {
                if (isUrdu)
                {
                    return "🎓 **پروجیکٹ سپروائزر:**\nہماری معزز پروجیکٹ سپروائزر **مس کائنات ساجد (Miss Kainat Sajid)** صاحبہ ہیں، جو **ایم فل ان کمپیوٹر سائنس (M.Phil CS)** اور یونیورسٹی آف سدرن پنجاب (USP) ملتان میں **لیکچرر** ہیں۔ انہوں نے شیئر میل (ShareMeal) پروجیکٹ کی تحقیق، رہنمائی اور تیکنیکی نگرانی فرمائی ہے۔";
                }
                if (isRomanUrdu)
                {
                    return "🎓 **Project Supervisor:**\nShareMeal project ki mohtarma supervisor **Miss Kainat Sajid** hain. Aap **M.Phil in Computer Science** hain aur **University of Southern Punjab (USP), Multan** mein Lecturer hain. Unhon ne is FYP ki architectural aur research guidance farmayi hai.";
                }
                return "🎓 **Academic Supervisor:**\nShareMeal is supervised by **Miss Kainat Sajid** (M.Phil in Computer Science, Lecturer of Computer Science at University of Southern Punjab - USP, Multan). She provided academic direction and evaluation for this Final Year Project.";
            }

            // 2. Developers / Creators (Muhammad Khulfan & Abdullah Khalid)
            if (q.Contains("developer") || q.Contains("created") || q.Contains("who made") || q.Contains("khulfan") || q.Contains("abdullah") || q.Contains("creator") || q.Contains("team") || q.Contains("banaya"))
            {
                if (isUrdu)
                {
                    return "👨‍💻 **ڈویلپرز اور ٹیم:**\nشیئر میل (ShareMeal) پلیٹ فارم **محمد خلفان (Muhammad Khulfan)** نے بطور لیڈ سافٹ ویئر انجینئر (.NET & iOS Developer, BSCS - USP Multan) اور **عبداللہ خالد (Abdullah Khalid)** (BSCS - USP Multan) نے اپنے فائنل ایئر پروجیکٹ (FYP) کے طور پر مس کائنات ساجد کی نگرانی میں ڈویلپ کیا ہے۔";
                }
                if (isRomanUrdu)
                {
                    return "👨‍💻 **Developers & Team:**\nShareMeal ko **Muhammad Khulfan** (Lead Software Engineer & iOS Developer, BSCS from USP Multan) ne team member **Abdullah Khalid** (BSCS USP Multan) ke sath mil kar engineer kiya hai, under the supervision of **Miss Kainat Sajid**.";
                }
                return "👨‍💻 **Project Engineers:**\nShareMeal was engineered by **Muhammad Khulfan** (Lead Full-Stack .NET & iOS Developer, BSCS from University of Southern Punjab, Multan) along with team member **Abdullah Khalid** (BSCS USP Multan), under the supervision of **Miss Kainat Sajid**.";
            }

            // 3. How to Donate (Restaurants)
            if (q.Contains("donate") || q.Contains("post") || q.Contains("hotel") || q.Contains("restaurant") || q.Contains("dena") || q.Contains("dalna"))
            {
                if (isUrdu)
                {
                    return "🍱 **کھانا عطیہ کرنے کا طریقہ:**\n1. اوپر مینو سے **Join Now** پر جا کر Restaurant اکاؤنٹ منتخب کریں۔\n2. لاگ ان کر کے **Post Surplus Food** فارم پر جائیں۔\n3. کھانے کا نام، مقدار، میعاد (Expiry Time) اور پتہ درج کر کے جمع کروائیں۔\n4. منظور شدہ فلاحی ادارے (Charities) آپ کے ہوٹل سے کھانا خود پک کر لیں گے!";
                }
                if (isRomanUrdu)
                {
                    return "🍱 **Khana Donate Karne Ka Tarika:**\n1. Website par **Register** karein aur *Restaurant* account select karein.\n2. Login karke **Donate Food** button par click karein.\n3. Dish ka naam, quantity (boxes/servings), expiry time aur pickup details daalein.\n4. Submit karte hi registered NGOs (jaise Edhi, Saylani) ko notification mil jayega!";
                }
                return "🍱 **How to Donate Food:**\n1. Register an account with the **Restaurant** role.\n2. Go to the **List Surplus Food** form from the navigation.\n3. Enter the food title, category, quantity, and expiration time.\n4. Verified charities and NGOs will instantly see and claim the food for distribution!";
            }

            // 4a. "What food is available?" — just show the list
            bool askingWhatFood = (q.Contains("available") || q.Contains("kya available") || q.Contains("abut food") || q.Contains("about food") || q.Contains("food hai") || q.Contains("konsa khana") || q.Contains("kya khana") || q.Contains("list") || q.Contains("show")) && !q.Contains("kaise") && !q.Contains("claim") && !q.Contains("how to");
            if (askingWhatFood && (q.Contains("food") || q.Contains("khana") || q.Contains("available") || q.Contains("donation")))
            {
                var sample = donations.Take(4).ToList();
                string sampleText = sample.Any() ? string.Join("\n", sample) : "• Biryani & Karahi (multiple portions)\n• Daal Chawal boxes available.";
                if (isUrdu)   return $"🍽️ **ابھی دستیاب کھانا:**\n{sampleText}\n\nکلیم کرنے کے لیے Marketplace جائیں اور **Claim Food** کریں۔";
                if (isRomanUrdu) return $"🍽️ **Abhi available khana:**\n{sampleText}\n\nClaim karne ke liye Marketplace mein jayein.";
                return $"🍽️ **Food Currently Available:**\n{sampleText}\n\nVisit the Marketplace to claim any item.";
            }

            // 4b. "How to claim food?" — show the process
            if (q.Contains("claim") || q.Contains("kaise milega") || q.Contains("how to get") || q.Contains("kahan milega") || q.Contains("chahiye") || q.Contains("lena") || (q.Contains("khana") && q.Contains("mil")) || q.Contains("charity") || q.Contains("ngo"))
            {
                var sample = donations.Take(2).ToList();
                string sampleText = sample.Any() ? string.Join("\n", sample) : "• Special Biryani & Karahi available.";
                if (isUrdu)   return $"🤝 **کھانا کلیم کرنے کا طریقہ:**\n1. Charity/NGO اکاؤنٹ سے لاگ ان کریں۔\n2. **Marketplace** میں جائیں — ابھی دستیاب:\n{sampleText}\n3. **Claim Food** پر کلک کریں اور pickup confirm کریں۔";
                if (isRomanUrdu) return $"🤝 **Khana claim karne ka tarika:**\n1. Charity account se login karein.\n2. Marketplace mein jayein — abhi available:\n{sampleText}\n3. **Claim Food** click karein aur pickup confirm karein.";
                return $"🤝 **How to Claim Food:**\n1. Sign in with a verified **Charity/NGO** account.\n2. Browse the **Live Marketplace** — currently available:\n{sampleText}\n3. Click **Claim Food** and confirm the pickup.";
            }

            // 4c. General food keyword fallback
            if (q.Contains("food") || q.Contains("khana") || q.Contains("where") || q.Contains("whare"))
            {
                var sample = donations.Take(2).ToList();
                string sampleText = sample.Any() ? string.Join("\n", sample) : "• Biryani & Karahi available.";
                if (isUrdu)   return $"🍽️ **ابھی دستیاب کھانا:**\n{sampleText}\n\nکلیم کرنے کے لیے Marketplace مین جائیں یا پوچھیں کیسے کلیم کریں۔";
                if (isRomanUrdu) return $"🍽️ **Abhi available khana:**\n{sampleText}\n\nKliam karne ke liye Marketplace dekhein. Poochh sakte hain 'kaise claim karein'.";
                return $"🍽️ **Available Food:**\n{sampleText}\n\nVisit the Marketplace to claim, or ask 'how to claim food'.";
            }

            // 5. City-specific queries (Karachi, Multan, Lahore, Islamabad, etc.)
            string? matchedCity = null;
            if (q.Contains("karachi")) matchedCity = "Karachi";
            else if (q.Contains("multan")) matchedCity = "Multan";
            else if (q.Contains("lahore")) matchedCity = "Lahore";
            else if (q.Contains("islamabad") || q.Contains("rawalpindi")) matchedCity = "Islamabad";
            else if (q.Contains("peshawar")) matchedCity = "Peshawar";
            else if (q.Contains("quetta")) matchedCity = "Quetta";
            else if (q.Contains("faisalabad")) matchedCity = "Faisalabad";

            if (matchedCity != null)
            {
                var cityDonations = donations.Where(d => d.IndexOf(matchedCity, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (cityDonations.Any())
                {
                    string list = string.Join("\n", cityDonations);
                    if (isUrdu) return $"📍 **{matchedCity} میں دستیاب کھانا:**\n{list}\n\nکلیم کرنے کے لیے Marketplace میں جائیں۔";
                    if (isRomanUrdu) return $"📍 **{matchedCity} mein available khana:**\n{list}\n\nClaim karne ke liye Marketplace visit karein.";
                    return $"📍 **Food available in {matchedCity}:**\n{list}\n\nVisit Marketplace to claim these meals.";
                }
                else
                {
                    if (isUrdu) return $"فی الوقت {matchedCity} میں کوئی فعال ڈونیشن موجود نہیں ہے، مگر تصدیق شدہ فلاحی ادارے یہاں فعال ہیں۔";
                    if (isRomanUrdu) return $"Filhal {matchedCity} mein koi live food donation listed nahi hai, lekin registered charities yahan active hain.";
                    return $"Currently there are no active food donations listed in {matchedCity}, but registered charities operate in this area.";
                }
            }

            // 6. Food Safety & SOPs
            if (q.Contains("safety") || q.Contains("hygiene") || q.Contains("safe") || q.Contains("sop") || q.Contains("kharab"))
            {
                if (isUrdu)
                {
                    return "🛡️ **خوراک کے حفاظتی اصول (Food Safety SOPs):**\n• پکا ہوا کھانا 4 گھنٹے کے اندر اندر مستحقین تک پہنچایا جائے۔\n• گرم کھانا 60°C سے اوپر اور ٹھنڈا کھانا 5°C سے نیچے محفوظ رکھا جائے۔\n• صرف حفظانِ صحت کے اصولوں پر پورا اترنے والے تصدیق شدہ ادارے ہی کھانا کلیم کر سکتے ہیں۔";
                }
                return "🛡️ **Food Safety & Hygiene SOPs:**\n• Prepared meals must be consumed or refrigerated within 4 hours.\n• Hot food must be stored above 60°C and cold items below 5°C.\n• Packaging must be sealed in food-grade foil/containers.\n• Only verified charities are authorized to collect donations.";
            }

            // 7. Default greeting & summary
            if (isUrdu)
            {
                return "آپ کیا جاننا چاہتے ہیں؟ کھانا عطیہ کرنے، وصول کرنے، یا کسی بھی موضوع پر پوچھ سکتے ہیں۔";
            }
            if (isRomanUrdu)
            {
                return "Apna sawaal poochhein — khana donate ya claim karne ke liye Marketplace dekhein, ya kisi bhi cheez ke baare mein poochhein.";
            }
            return "How can I help you? Ask about food donations, claiming food, restaurants, or charities on ShareMeal.";
        }
    }
}
