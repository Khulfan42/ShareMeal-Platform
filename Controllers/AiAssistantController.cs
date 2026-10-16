using Microsoft.AspNetCore.Mvc;
using ShareMeal.Web.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShareMeal.Web.Controllers
{
    public class AiAssistantController : Controller
    {
        private readonly IGeminiAiService _geminiService;
        private readonly IConfiguration _configuration;

        public AiAssistantController(IGeminiAiService geminiService, IConfiguration configuration)
        {
            _geminiService = geminiService;
            _configuration = configuration;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        public class ApiKeySaveRequest
        {
            public string ApiKey { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return Json(new { status = "error", reply = "Please provide a valid question." });

            var answer = await _geminiService.AskAssistantAsync(request.Message.Trim());
            return Json(new { status = "success", reply = answer });
        }

        [HttpGet]
        public IActionResult GetApiKeyStatus()
        {
            string? key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? _configuration["Gemini:ApiKey"];
            string model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? _configuration["Gemini:Model"] ?? "gemini-3.5-flash-lite";
            bool isConfigured = !string.IsNullOrWhiteSpace(key) && key.Length > 20 && !key.Contains("YOUR_GEMINI_API_KEY");
            string masked = "";
            if (isConfigured && key != null)
            {
                masked = key.Length > 10 ? key.Substring(0, 6) + "••••••••" + key.Substring(key.Length - 4) : "••••••••";
            }
            return Json(new { configured = isConfigured, maskedKey = masked, model = model });
        }

        [HttpPost]
        public IActionResult SaveApiKey([FromBody] ApiKeySaveRequest request)
        {
            string key = (request?.ApiKey ?? string.Empty).Trim();
            string model = (request?.Model ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(model)) model = "gemini-3.5-flash-lite";

            if (string.IsNullOrWhiteSpace(key))
                return Json(new { success = false, message = "Please enter a valid API key." });

            try
            {
                var files = new[] { "appsettings.json", "appsettings.Development.json" };
                foreach (var f in files)
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), f);
                    if (System.IO.File.Exists(filePath))
                    {
                        string json = System.IO.File.ReadAllText(filePath);
                        var node = JsonNode.Parse(json);
                        if (node != null)
                        {
                            var geminiNode = node["Gemini"] as JsonObject;
                            if (geminiNode == null)
                            {
                                geminiNode = new JsonObject();
                                node["Gemini"] = geminiNode;
                            }

                            geminiNode["ApiKey"] = key;
                            geminiNode["Model"] = model;

                            System.IO.File.WriteAllText(filePath,
                                node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                Environment.SetEnvironmentVariable("GEMINI_API_KEY", key);
                Environment.SetEnvironmentVariable("GEMINI_MODEL", model);
                return Json(new { success = true, message = "API Key saved! MealBot ab live Gemini AI use karega." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
