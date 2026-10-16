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

                var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? _config["Gemini:ApiKey"];
                var preferredModel = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? _config["Gemini:Model"] ?? "gemini-3.5-flash-lite";

                bool hasValidApiKey = !string.IsNullOrWhiteSpace(apiKey) && 
                                      !apiKey.Contains("YOUR_GEMINI_API_KEY") && 
                                      !apiKey.StartsWith("YOUR_") &&
                                      apiKey.Length > 20;

                // If real key exists, try calling Google Gemini API first
                if (hasValidApiKey)
                {
                    var donationsContext = availableDonations.Any() ? string.Join("\n", availableDonations) : "No active donations currently listed.";
                    var restaurantsContext = verifiedRestaurants.Any() ? string.Join(", ", verifiedRestaurants) : "Various verified restaurants across Pakistan.";
                    var charitiesContext = verifiedCharities.Any() ? string.Join(", ", verifiedCharities) : "Edhi, Saylani, Al-Khidmat, Chhipa, JDC.";

                    var systemInstruction = @"You are MealBot, an intelligent and versatile AI assistant for ShareMeal Pakistan and all general topics.
You behave just like ChatGPT: answer ANY question thoroughly, accurately, and politely (programming, coding, computer science, general knowledge, daily life, advice, recipes, science, or ShareMeal food rescue platform features).

LIVE SHAREMEAL CONTEXT (use this ONLY when the user asks about food donations, restaurants, or charities):
Active Donations: " + donationsContext + @"
Restaurants: " + restaurantsContext + @"
Charities: " + charitiesContext + @"

CRITICAL RULES:
1. Directly answer the user's specific question with rich, clear, and structured detail.
2. STRICT RULE ON CREATORS: DO NOT mention Muhammad Khulfan, Abdullah Khalid, or Miss Kainat Sajid unless the user EXPLICITLY asks who made/developed/engineered the project (e.g. 'developer kaun hai', 'who made this', 'kisne banaya'). In all other responses (programming, registration, food, etc.), NEVER mention them at all.
3. Language: Reply in the same language the user used (Roman Urdu, Urdu script, or English).";

                    var candidateModels = new List<string> {
                        "gemini-3.5-flash-lite",
                        "gemini-robotics-er-2-preview",
                        "gemini-3.1-flash-lite",
                        preferredModel,
                        "gemini-3.8-flash",
                        "gemini-3.6-flash"
                    }.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();

                    foreach (var m in candidateModels)
                    {
                        try
                        {
                            var requestBody = new
                            {
                                contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
                                systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                                generationConfig = new
                                {
                                    temperature = 0.3,
                                    maxOutputTokens = 1000
                                }
                            };

                            var jsonPayload = JsonSerializer.Serialize(requestBody);
                            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={apiKey}";
                            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                            var resp = await _httpClient.SendAsync(req, cts.Token);
                            if (resp.IsSuccessStatusCode)
                            {
                                var contentStr = await resp.Content.ReadAsStringAsync();
                                using var doc = JsonDocument.Parse(contentStr);
                                if (doc.RootElement.TryGetProperty("candidates", out var cands) && cands.GetArrayLength() > 0)
                                {
                                    var first = cands[0];
                                    if (first.TryGetProperty("content", out var candContent) && candContent.TryGetProperty("parts", out var p) && p.GetArrayLength() > 0)
                                    {
                                        var text = p[0].GetProperty("text").GetString();
                                        if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                                    }
                                }
                            }
                            // If 429 or 503, try next candidate model
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Gemini API call with model {Model} failed: {Msg}", m, ex.Message);
                        }
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

            // Registration / Sign Up query
            if (q.Contains("register") || q.Contains("sign up") || q.Contains("signup") || q.Contains("account") || q.Contains("join") || q.Contains("login") || q.Contains("shamil"))
            {
                if (isUrdu)
                {
                    return "📝 **شیئر میل پر رجسٹریشن کا طریقہ:**\n1. اوپر نیویگیشن بار میں **Join Now** یا **Register** پر کلک کریں۔\n2. اپنا رول منتخب کریں:\n   • **Restaurant / Donor:** کھانا عطیہ کرنے کے لیے۔\n   • **Charity / NGO:** کھانا کلیم اور تقسیم کرنے کے لیے۔\n3. اپنا نام، ادارہ، ای میل اور پاس ورڈ درج کر کے سبمٹ کریں۔\n4. تصدیق کے بعد آپ کا اکاؤنٹ فعال ہو جائے گا!";
                }
                return "📝 **ShareMeal Par Register Karne Ka Asaan Tarika:**\n1. Website ke top menu mein **Join Now** ya **Register** par click karein.\n2. Apna Role select karein:\n   • **Restaurant / Hotel:** Agar aap surplus food donate karna chahte hain.\n   • **Charity / NGO:** Agar aap khana claim kar ke zaroorat-mandon mein taqseem karna chahte hain.\n3. Apna Naam, Organization Name, Email, Address aur Password enter kar ke **Register** button dabayein.\n4. Register hone ke baad foran login karein aur khana donate ya claim karna shuru karein!";
            }

            // 2. Developers / Creators (Muhammad Khulfan & Abdullah Khalid) - ONLY when asked!
            if (q.Contains("developer") || q.Contains("created") || q.Contains("who made") || q.Contains("khulfan") || q.Contains("abdullah") || q.Contains("creator") || (q.Contains("kisne") && q.Contains("banaya")))
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

            // 7. Fallback when AI models are unavailable
            if (isUrdu)
            {
                return "براہِ کرم MealBot کے اوپر 🔑 (Key Icon) پر کلک کر کے اپنی نئی Google AI Studio API Key درج کریں تاکہ لائیو AI جوابات حاصل کر سکیں۔";
            }
            if (isRomanUrdu)
            {
                return "⚠️ Google Gemini API key ka daily free quota poora ho chuka hai. Barah-e-karam MealBot ke header mein 🔑 (Key Icon) par click kar ke apni nayi Google AI Studio API Key paste karein taake live AI jawab deta rahe!";
            }
            return "⚠️ Gemini AI key quota has been exhausted. Please click the 🔑 (Key Icon) in the header to enter a fresh Google AI Studio API key!";
        }
    }
}
