using System.ComponentModel.DataAnnotations;

namespace ShareMeal.Web.Models;

public class Organization
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public OrganizationType Type { get; set; }

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public OrganizationStatus Status { get; set; } = OrganizationStatus.Pending;

    public bool IsSuspended { get; set; } = false;

    // Link to IdentityUser
    public string? OwnerId { get; set; }

    // Navigation properties
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
