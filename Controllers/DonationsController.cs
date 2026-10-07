using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;

namespace ShareMeal.Web.Controllers;

public class DonationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DonationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Donations
    public async Task<IActionResult> Index()
    {
        var availableDonations = await _context.Donations
            .Include(d => d.Donor)
            .Where(d => d.Status == DonationStatus.Available)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(availableDonations);
    }

    // GET: Donations/MyActivity
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> MyActivity()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        var myDonations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Recipient)
            .Where(d => d.DonorId == org.Id || d.RecipientId == org.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(myDonations);
    }

    // GET: Donations/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var donation = await _context.Donations
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (donation == null)
        {
            TempData["ErrorMessage"] = "The requested donation was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(donation);
    }

    // GET: Donations/Create
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Create()
    {
        var donation = new Donation
        {
            ExpiryDate = DateTime.Now.AddHours(24)
        };
        return View(donation);
    }

    // POST: Donations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Create([Bind("FoodItem,Category,Quantity,ExpiryDate,PickupInstructions")] Donation donation)
    {
        // Remove validation for fields we set manually
        ModelState.Remove("DonorId");
        ModelState.Remove("Donor");

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
            if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
            if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(); }
            
            // Auto-provision a default restaurant profile if user has none
            if (org == null)
            {
                var orgName = (user.Email ?? "Restaurant").Split('@')[0];
                org = new Organization
                {
                    Name = char.ToUpper(orgName[0]) + orgName.Substring(1) + " Restaurant",
                    Type = OrganizationType.Restaurant,
                    ContactEmail = user.Email ?? "donor@sharemeal.org",
                    OwnerId = user.Id,
                    Status = OrganizationStatus.Verified,
                    Address = "Pakistan",
                    Phone = "+92 300 1234567"
                };
                _context.Organizations.Add(org);
                await _context.SaveChangesAsync();
            }

            donation.DonorId = org.Id;
            donation.CreatedAt = DateTime.UtcNow;
            donation.Status = DonationStatus.Available;

            _context.Add(donation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Food donation '{donation.FoodItem}' successfully listed! 🎉";
            return RedirectToAction(nameof(Index));
        }
        return View(donation);
    }

    // GET: Donations/RequestFood/5
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> RequestFood(int? id)
    {
        Donation? donation = null;
        if (id.HasValue && id.Value > 0)
        {
            donation = await _context.Donations
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(m => m.Id == id.Value);
        }
        else
        {
            // Pick first available donation if id was omitted in URL
            donation = await _context.Donations
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(m => m.Status == DonationStatus.Available);
        }
            
        if (donation == null || donation.Status != DonationStatus.Available)
        {
            TempData["ErrorMessage"] = "No available food donation found to request at this time.";
            return RedirectToAction(nameof(Index));
        }

        return View(donation);
    }

    // POST: Donations/Claim/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Claim(int id)
    {
        var donation = await _context.Donations.FindAsync(id);
        if (donation == null || donation.Status != DonationStatus.Available)
        {
            TempData["ErrorMessage"] = "This food item is no longer available or was already claimed.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO); }
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(); }
        
        // Auto-provision a default charity organization if needed
        if (org == null)
        {
            var orgName = (user.Email ?? "Charity").Split('@')[0];
            org = new Organization
            {
                Name = char.ToUpper(orgName[0]) + orgName.Substring(1) + " Relief Foundation",
                Type = OrganizationType.Charity,
                ContactEmail = user.Email ?? "charity@sharemeal.org",
                OwnerId = user.Id,
                Status = OrganizationStatus.Verified,
                Address = "Pakistan",
                Phone = "+92 300 1234567"
            };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
        }

        donation.RecipientId = org.Id;
        donation.Status = DonationStatus.Claimed;

        _context.Update(donation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Successfully claimed '{donation.FoodItem}'! Please coordinate pickup.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Donations/MarkAsCollected/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> MarkAsCollected(int id)
    {
        var donation = await _context.Donations.FindAsync(id);
        if (donation == null || donation.Status != DonationStatus.Claimed)
        {
            TempData["ErrorMessage"] = "Donation item not found.";
            return RedirectToAction(nameof(Index));
        }

        donation.Status = DonationStatus.Collected;
        _context.Update(donation);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Food marked as Collected & Distributed! ✅";

        var referer = Request.Headers["Referer"].ToString();
        return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/");
    }

    // POST: Donations/CancelPickup/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CancelPickup(int id)
    {
        var donation = await _context.Donations.FindAsync(id);
        if (donation == null || donation.Status != DonationStatus.Claimed) return NotFound();

        // Revert to Available and remove Recipient
        donation.Status = DonationStatus.Available;
        donation.RecipientId = null;
        _context.Update(donation);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Pickup reservation cancelled and food returned to marketplace.";

        var referer = Request.Headers["Referer"].ToString();
        return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/");
    }
}
