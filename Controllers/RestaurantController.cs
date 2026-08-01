using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;

namespace ShareMeal.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class RestaurantController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public RestaurantController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
            org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant);
        }

        if (org == null) return RedirectToAction("Index", "Home");

        if (org.Status != OrganizationStatus.Verified)
        {
            return RedirectToAction("AccountStatus", "Home", new { status = org.Status });
        }

        ViewBag.TotalDonations = await _context.Donations.CountAsync(d => d.DonorId == org.Id);
        ViewBag.PendingRequests = await _context.Donations.CountAsync(d => d.DonorId == org.Id && d.Status == DonationStatus.Claimed);
        ViewBag.CompletedDonations = await _context.Donations.CountAsync(d => d.DonorId == org.Id && d.Status == DonationStatus.Collected);
        
        return View();
    }

    // GET: Restaurant/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        return View(org);
    }

    // POST: Restaurant/Profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(Organization model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
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

    // List of food posted by restaurant
    public async Task<IActionResult> MyDonations()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        var donations = await _context.Donations
            .Include(d => d.Recipient)
            .Where(d => d.DonorId == org.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(donations);
    }

    public async Task<IActionResult> DonationRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        var requests = await _context.Donations
            .Include(d => d.Recipient)
            .Where(d => d.DonorId == org.Id && d.Status == DonationStatus.Claimed)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(requests);
    }

    public async Task<IActionResult> PickupSchedule()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        var pickups = await _context.Donations
            .Include(d => d.Recipient)
            .Where(d => d.DonorId == org.Id && d.Status == DonationStatus.Claimed)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();
            
        return View(pickups);
    }

    public async Task<IActionResult> FoodTracking()
    {
        var user = await _userManager.GetUserAsync(User);
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.OwnerId == user.Id);
        if (org == null && User.IsInRole("Admin")) { org = await _context.Organizations.FirstOrDefaultAsync(o => o.Type == OrganizationType.Restaurant); }
        if (org == null) return RedirectToAction("Index", "Home");

        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Recipient)
            .Where(d => d.DonorId == org.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return View(donations);
    }
}
