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
        // For demonstration/FYP, we log the email to the console.
        // In a real app, you would use SendGrid or an SMTP client here.
        
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine($"📧 EMAIL SENT TO: {email}");
        Console.WriteLine($"📌 SUBJECT: {subject}");
        Console.WriteLine($"📝 MESSAGE: {message}");
        Console.WriteLine(new string('=', 50) + "\n");

        _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);
        
        return Task.CompletedTask;
    }
}
