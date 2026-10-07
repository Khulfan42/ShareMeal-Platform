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
            _config = config;
            _context = context;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
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
                var model = _config["Gemini:Model"] ?? "gemini-2.0-flash";

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

                        var systemInstruction = $@"You are MealBot AI, the intelligent virtual assistant for ShareMeal Platform across Pakistan.
Created by Muhammad Khulfan (Lead Developer, iOS & .NET Engineer, BSCS from University of Southern Punjab USP, Multan) and Abdullah Khalid (BSCS USP Multan), under the supervision of Miss Kainat Sajid (M.Phil Computer Science, Lecturer at USP Multan).

Available Food in DB: {donationsContext}
Restaurants: {restaurantsContext}
Charities: {charitiesContext}

GUIDELINES:
- If user writes in Urdu script, reply in fluent Urdu script.
- If user writes in Roman Urdu, reply in friendly Roman Urdu.
- If user writes in English, reply in professional English.
- Be concise (under 130 words), polite, and helpful with emojis.";

                        var requestBody = new
                        {
                            contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
                            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                            generationConfig = new { temperature = 0.7, maxOutputTokens = 600 }
                        };

                        var jsonPayload = JsonSerializer.Serialize(requestBody);
                        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var resp = await _httpClient.SendAsync(req);
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
                return "Assalam-o-Alaikum! Welcome to ShareMeal Platform. You can explore available food donations in the marketplace or register your restaurant/charity.";
            }
        }

        private string GenerateSmartReply(string query, List<string> donations, List<string> restaurants, List<string> charities)
        {
            var q = query.ToLower().Trim();
            bool isUrdu = Regex.IsMatch(query, @"[\u0600-\u06FF]");
            bool isRomanUrdu = Regex.IsMatch(q, @"\b(kya|kaun|kon|kaise|kahan|kha|kese|hain|hai|bhai|ap|aap|khana|khatam|supervisor|madam|batao|batayein|mil|milega|chahiye|shukriya|kesa)\b");

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

            // 4. How to Claim / Request Food (Charities)
            if (q.Contains("claim") || q.Contains("request") || q.Contains("charity") || q.Contains("ngo") || q.Contains("khana kahan") || q.Contains("chahiye") || q.Contains("lena"))
            {
                var sample = donations.Take(2).ToList();
                string sampleText = sample.Any() ? string.Join("\n", sample) : "• Special Biryani & Karahi available.";

                if (isUrdu)
                {
                    return $"🤝 **کھانا وصول کرنے کا طریقہ:**\n1. خیراتی ادارے (Charity/NGO) کے طور پر رجسٹر ہوں۔\n2. **Marketplace** میں جائیں جہاں دستیاب کھانا موجود ہے:\n{sampleText}\n3. کسی بھی کھانے پر **Claim Food** پر کلک کریں اور ریسٹورنٹ سے رابطہ کر کے کھانا حاصل کریں!";
                }
                if (isRomanUrdu)
                {
                    return $"🤝 **Khana Claim Karne Ka Tarika:**\n1. **Charity** account se login karein.\n2. **Donations Marketplace** browse karein jahan live khana available hai:\n{sampleText}\n3. Apni pasand ke item par **Claim Food** click karein aur pickup confirm karein!";
                }
                return $"🤝 **How Charities Claim Food:**\n1. Sign in with a verified **Charity/NGO** account.\n2. Browse the **Live Donations Marketplace**:\n{sampleText}\n3. Click **Claim Food** to lock the reservation and view pickup instructions!";
            }

            // 5. Nationwide / Cities (Multan, Lahore, Karachi, Islamabad, etc.)
            if (q.Contains("multan") || q.Contains("lahore") || q.Contains("karachi") || q.Contains("islamabad") || q.Contains("peshawar") || q.Contains("quetta") || q.Contains("faisalabad") || q.Contains("city") || q.Contains("shehar"))
            {
                if (isUrdu)
                {
                    return "🇵🇰 **ملک گیر کوریج:**\nشیئر میل پورے پاکستان میں فعال ہے! بشمول ملتان (USP ہوم)، لاہور، کراچی، اسلام آباد، راولپنڈی، پشاور، کوئٹہ اور فیصل آباد۔ سیلانی ویلفیئر، ایدھی فاؤنڈیشن، اور الخدمت فاؤنڈیشن ہمارے تصدیق شدہ شراکت دار ہیں۔";
                }
                if (isRomanUrdu)
                {
                    return "🇵🇰 **Nationwide Coverage across Pakistan:**\nShareMeal poore Pakistan mein active hai! Multan (home of USP), Lahore, Karachi, Islamabad, Rawalpindi, Peshawar, Quetta, Faisalabad aur Sialkot. Savour Foods, Monal, Bundu Khan, Saylani, Edhi aur Al-Khidmat verified partners hain!";
                }
                return "🇵🇰 **Nationwide Coverage Across Pakistan:**\nShareMeal operates nationwide across Islamabad, Rawalpindi, Lahore, Karachi, Multan (home of USP), Peshawar, Faisalabad, and Quetta, partnering with renowned restaurants and verified charities including Edhi, Saylani, and Al-Khidmat.";
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
                return "السلام علیکم! شیئر میل (ShareMeal) میں خوش آمدید۔ آپ زائد کھانا عطیہ (Donate) کر سکتے ہیں، یا مستحقین کے لیے کلیم (Claim) کر سکتے ہیں۔ آپ ریسٹورنٹس، خیراتی اداروں یا پروجیکٹ کے متعلق کچھ بھی پوچھ سکتے ہیں!";
            }
            if (isRomanUrdu)
            {
                return "Assalam-o-Alaikum! Welcome to ShareMeal Pakistan. Aap yahan se surplus food donate kar sakte hain ya charities ke zariye claim kar sakte hain. Aap kisi bhi shehar ya project ke bare mein sawaal pooch sakte hain!";
            }
            return "Welcome to ShareMeal Pakistan! We connect restaurants, hotels, and caterers with verified relief charities to eliminate food waste and fight hunger. How can I assist you today?";
        }
    }
}
