namespace ShareMeal.Web.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        // Transactional email logging provider for development and testing.
        // Dispatches simulated notification logs to standard console output.
        
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine($"📧 EMAIL SENT TO: {email}");
        Console.WriteLine($"📌 SUBJECT: {subject}");
        Console.WriteLine($"📝 MESSAGE: {message}");
        Console.WriteLine(new string('=', 50) + "\n");

        _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);
        
        return Task.CompletedTask;
    }
}
