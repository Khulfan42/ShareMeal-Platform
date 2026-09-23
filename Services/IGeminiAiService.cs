namespace ShareMeal.Web.Services
{
    public interface IGeminiAiService
    {
        Task<string> AskAssistantAsync(string userMessage);
    }
}
