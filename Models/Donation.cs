using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareMeal.Web.Models;

public class Donation
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FoodItem { get; set; } = string.Empty;

    [Required]
    public FoodCategory Category { get; set; }

    [Required]
    public string Quantity { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    public string? PickupInstructions { get; set; }

    public DonationStatus Status { get; set; } = DonationStatus.Available;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key for Restaurant (Donor)
    [Required]
    public int DonorId { get; set; }
    
    [ForeignKey("DonorId")]
    public Organization? Donor { get; set; }

    // Foreign Key for NGO (Claimer) - nullable until claimed
    public int? RecipientId { get; set; }

    [ForeignKey("RecipientId")]
    public Organization? Recipient { get; set; }
}
