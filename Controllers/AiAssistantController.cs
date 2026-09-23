using Microsoft.AspNetCore.Mvc;
using ShareMeal.Web.Services;

namespace ShareMeal.Web.Controllers
{
    public class AiAssistantController : Controller
    {
        private readonly IGeminiAiService _geminiService;

        public AiAssistantController(IGeminiAiService geminiService)
        {
            _geminiService = geminiService;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return Json(new { status = "error", reply = "Please provide a valid question." });
            }

            var answer = await _geminiService.AskAssistantAsync(request.Message.Trim());
            return Json(new { status = "success", reply = answer });
        }
    }
}
