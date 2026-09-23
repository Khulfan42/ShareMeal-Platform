using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;

namespace ShareMeal.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Charity,Admin")]
public class CharityController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CharityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Main Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);

        if (org == null && User.IsInRole("Admin"))
        {
            org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO);
        }

        if (org == null) return RedirectToAction("Index", "Home");

        if (org.Status != OrganizationStatus.Verified)
        {
            return RedirectToAction("AccountStatus", "Home", new { status = org.Status });
        }

        ViewBag.TotalClaimed = await _context.Donations.CountAsync(d => d.RecipientId == org.Id);
        ViewBag.PendingPickups = await _context.Donations.CountAsync(d => d.RecipientId == org.Id && d.Status == DonationStatus.Claimed);
        ViewBag.MealsCollected = await _context.Donations.CountAsync(d => d.RecipientId == org.Id && d.Status == DonationStatus.Collected);
        
        return View();
    }

    // GET: Charity/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO); }
        if (org == null) return RedirectToAction("Index", "Home");

        return View(org);
    }

    // POST: Charity/Profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(Organization model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO); }
        if (org == null) return RedirectToAction("Index", "Home");

        org.Name = model.Name;
        org.Phone = model.Phone;
        org.Address = model.Address;
        org.Description = model.Description;

        _context.Update(org);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> MyRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO); }
        if (org == null) return RedirectToAction("Index", "Home");

        var requests = await _context.Donations
            .Include(d => d.Donor)
            .Where(d => d.RecipientId == org.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(requests);
    }

    public async Task<IActionResult> PickupDetails()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO); }
        if (org == null) return RedirectToAction("Index", "Home");

        var pickups = await _context.Donations
            .Include(d => d.Donor)
            .Where(d => d.RecipientId == org.Id && d.Status == DonationStatus.Claimed) 
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();
            
        return View(pickups);
    }
}
