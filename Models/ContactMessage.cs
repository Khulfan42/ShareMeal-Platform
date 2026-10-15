using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareMeal.Web.Models;

/// <summary>
/// Contact form submissions from website visitors or registered users.
/// ERD ke mutabiq: Name, Email, Subject, Message, SubmittedAt, IsResolved, UserId (optional FK)
/// </summary>
public class ContactMessage
{
    public int Id { get; set; }

    // Visitor ya user ka naam
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Contact email address
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // Message ka subject/topic
    [Required(ErrorMessage = "Subject is required")]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    // Full message content
    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;

    // Submission timestamp
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Admin ne resolve kiya ya nahi
    public bool IsResolved { get; set; } = false;

    // Optional: Logged-in user ka reference (null for guests)
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }
}
