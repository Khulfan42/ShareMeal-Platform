using System.Text;
using System.Text.Json;
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
        }

        public async Task<string> AskAssistantAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a question or topic about ShareMeal.";
            }

            try
            {
                var apiKey = _config["Gemini:ApiKey"];
                var model = _config["Gemini:Model"] ?? "gemini-3.6-flash";

                if (string.IsNullOrEmpty(apiKey))
                {
                    return "Gemini API key is not configured in appsettings.json.";
                }

                // 1. Gather live database platform context across all major Pakistani cities
                var availableDonations = await _context.Donations
                    .Include(d => d.Donor)
                    .Where(d => d.Status == DonationStatus.Available)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(30)
                    .Select(d => $"• {d.FoodItem} ({d.Quantity}) — Donor: {d.Donor.Name} ({d.Donor.Address ?? "Pakistan"})")
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

                var donationsContext = availableDonations.Any()
                    ? string.Join("\n", availableDonations)
                    : "No active donations available at this exact moment.";

                var charitiesContext = string.Join("; ", verifiedCharities);
                var restaurantsContext = string.Join("; ", verifiedRestaurants);

                // 2. Build structured system prompt
                var systemInstruction = $@"You are 'MealBot', the official AI Food Relief Assistant for ShareMeal Pakistan (Final Year Project by Muhammad Khulfan).
ShareMeal is Pakistan's premier nationwide surplus food distribution network connecting hotels, restaurants, and catering services with registered welfare organizations and charities (e.g. Saylani, Edhi, Al-Khidmat, Chhipa, JDC, Rizq Trust, Multan Khidmat Dastarkhwan).

NATIONWIDE COVERAGE:
ShareMeal is NOT restricted to Islamabad. It is an active nationwide platform covering ALL MAJOR CITIES AND PROVINCES OF PAKISTAN:
- Islamabad & Rawalpindi (Twin Cities / Federal Capital & Potohar)
- Lahore (Punjab)
- Karachi (Sindh)
- Multan (South Punjab - Special home of Muhammad Khulfan's alma mater, University of Southern Punjab USP!)
- Peshawar (Khyber Pakhtunkhwa)
- Faisalabad (Punjab)
- Quetta (Balochistan)
- Gujranwala & Sialkot (Punjab)
- Hyderabad (Sindh)
- And any other city where restaurants or charities register!

LIVE DATABASE SNAPSHOT ACROSS PAKISTANI CITIES:
Active Available Surplus Food in Marketplace:
{donationsContext}

Verified Partner Restaurants across Pakistan:
{restaurantsContext}

Registered Relief Charities & NGOs across Pakistan:
{charitiesContext}

GUIDELINES:
1. NATIONWIDE SCOPE (Crucial):
   - When a user asks about food availability, donor hotels, or relief charities in ANY city (e.g., Multan, Lahore, Karachi, Peshawar, Quetta, Faisalabad, Rawalpindi, Gujranwala, Sialkot, Hyderabad):
     * Cite the exact dishes, quantities, and restaurants registered in that city from the live database snapshot above.
     * Highlight local registered charities (like Saylani in Karachi/Faisalabad, Al-Khidmat & Rizq in Lahore, Multan Khidmat & USP Welfare in Multan, SRSP in Peshawar, Balochistan Hunger Network in Quetta, Chhipa in Karachi, etc.).
     * If the user mentions any town or asks general questions like 'Pakistan ke kis kis shehar mein kaam karta hai?', proudly declare that ShareMeal covers all of Pakistan and any hotel/NGO in Pakistan can sign up!
2. STRICT LANGUAGE MATCHING RULE (Crucial):
   - If the user asks in Urdu script (e.g., 'کھانا کہاں ملے گا؟' / 'ملتان میں کون سا کھانا دستیاب ہے؟'), you MUST reply strictly in elegant Urdu script (اردو).
   - If the user asks in Roman Urdu (e.g., 'Multan ya Lahore mein khana kahan available hai?', 'Muhammad Khulfan kon hai?'), you MUST reply strictly in friendly, natural Roman Urdu.
   - If the user asks in English (e.g., 'Where is food available in Karachi or Peshawar?', 'Who created this platform?'), you MUST reply strictly in fluent, professional English.
   - NEVER reply in English if the user communicated in Urdu or Roman Urdu! Match their language 100%.
3. Developer & Creator Profile:
   - ShareMeal was architected and built by **Muhammad Khulfan** with team member **Abdullah Khalid**.
   - **Muhammad Khulfan**: Professional **iOS Engineer** & Full Stack .NET Developer. Degree: **BSCS** from **University of Southern Punjab (USP)**, Multan. He is the lead developer who engineered ShareMeal using ASP.NET Core, SQLite, and Google Gemini AI as his Final Year Project (FYP).
   - **Abdullah Khalid**: Team Member & Developer. Degree: **BSCS (Bachelor of Science in Computer Science)**. He contributed to the development of ShareMeal Platform as part of the project team.
   - When asked 'Who made this?', 'Developer kaun hai?', 'Abdullah Khalid kon hai?', or about either developer, proudly introduce both of them with their exact credentials.
4. Expertise: Answer questions on:
   - How restaurants anywhere in Pakistan donate (Login -> Restaurant Panel -> '+ Post New Donation').
   - How charities claim (Browse Marketplace -> Click 'Claim Food' -> Coordinate pickup).
   - Food Safety SOPs (Prepared meals should be consumed within 4 hours; keep hot food >60°C and cold food <5°C).
   - Platform Verification (Admin approves restaurants & charities to ensure food hygiene & trust).
5. Tone: Helpful, enthusiastic, professional, and concise (under 140 words unless detailed explanation is asked). Use bullet points and emojis where appropriate.";

                // 3. Prepare Gemini API Request
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = userMessage }
                            }
                        }
                    },
                    systemInstruction = new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 800
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                var modelsToTry = new[] { model, "gemini-3.5-flash-lite", "gemini-3.6-flash", "gemini-flash-latest" }.Distinct().ToArray();

                foreach (var currentModel in modelsToTry)
                {
                    try
                    {
                        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent?key={apiKey}";
                        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var response = await _httpClient.SendAsync(request);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            using var doc = JsonDocument.Parse(responseContent);
                            var root = doc.RootElement;

                            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                            {
                                var firstCandidate = candidates[0];
                                if (firstCandidate.TryGetProperty("content", out var content) &&
                                    content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                                {
                                    var text = parts[0].GetProperty("text").GetString();
                                    if (!string.IsNullOrWhiteSpace(text)) return text;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Model {Model} returned {Status}, trying next fallback...", currentModel, response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error with model {Model}", currentModel);
                    }
                }

                return "AI Assistant is currently busy. Please try asking again in a few seconds.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Gemini AI assistant");
                return "AI Assistant encountered an error while processing your request. Please try again.";
            }
        }
    }
}
