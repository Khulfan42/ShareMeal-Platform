using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;
using ShareMeal.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ShareMeal.Web.Controllers;

// NOTE: [Authorize] is applied per-action, NOT on the class, so AdminLogin page stays public
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AdminController(ApplicationDbContext context, IEmailService emailService, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _emailService = emailService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // =============================================
    // DEDICATED ADMIN LOGIN - Private Entry Point
    // =============================================
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(nameof(Dashboard));
            }
            // Logged in but not Admin - sign them out and show error
            await _signInManager.SignOutAsync();
            ViewBag.Error = "Access Denied. This portal is for Admins only.";
            return View();
        }
        ViewBag.Error = "Invalid email or password. Please try again.";
        return View();
    }

    // Main Dashboard
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _context.Organizations.CountAsync();
        ViewBag.TotalDonations = await _context.Donations.CountAsync();
        ViewBag.SuccessRate = 0; // Prevent divide by zero
        if(ViewBag.TotalDonations > 0)
        {
            var completed = await _context.Donations.CountAsync(d => d.Status == DonationStatus.Collected);
            ViewBag.SuccessRate = (int)((double)completed / ViewBag.TotalDonations * 100);
        }
        
        return View();
    }

    // Manage Restaurants and Charities (Combined for scale, but split for presentation)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();
        return View(users);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageRestaurants()
    {
        var users = await _context.Organizations.Where(o => o.Type == OrganizationType.Restaurant).ToListAsync();
        return View(users);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageCharities()
    {
        var users = await _context.Organizations.Where(o => o.Type == OrganizationType.Charity || o.Type == OrganizationType.NGO).ToListAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOrganization(int id)
    {
        var org = await _context.Organizations.FindAsync(id);
        if (org != null)
        {
            org.Status = OrganizationStatus.Verified;
            org.IsSuspended = false;
            _context.Update(org);
            await _context.SaveChangesAsync();

            // Send Approval Email
            string subject = "ShareMeal - Organization Approved! 🎉";
            string message = $@"Hi {org.Name},

Congratulations! Your organization has been verified by the ShareMeal admin team.

You now have full access to your Dashboard where you can post food donations or claim available items.

Welcome to our mission!
The ShareMeal Team";

            await _emailService.SendEmailAsync(org.ContactEmail, subject, message);
        }
        var referer = Request.Headers["Referer"].ToString();
        return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/Admin/Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> SuspendOrganization(int id)
    {
        var org = await _context.Organizations.FindAsync(id);
        if (org != null)
        {
            org.Status = OrganizationStatus.Suspended;
            org.IsSuspended = true;
            _context.Update(org);
            await _context.SaveChangesAsync();

            // Send Suspension Email
            string subject = "ShareMeal - Account Suspended ⚠️";
            string message = $@"Hi {org.Name},

Your organization account on ShareMeal has been suspended by the administrator. 

During this time, you will not be able to access your dashboard, post new food donations, or claim items. 

If you believe this is a mistake, please contact our support team at support@sharemeal.com or via WhatsApp at 03037377365.

Regards,
The ShareMeal Team";

            await _emailService.SendEmailAsync(org.ContactEmail, subject, message);
        }
        var referer = Request.Headers["Referer"].ToString();
        return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/Admin/Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        var org = await _context.Organizations.FindAsync(id);
        if (org != null)
        {
            _context.Organizations.Remove(org);
            await _context.SaveChangesAsync();
        }
        var referer = Request.Headers["Referer"].ToString();
        return Redirect(!string.IsNullOrEmpty(referer) ? referer : "/Admin/Dashboard");
    }

    // GET: Admin/OrganizationDetails/5
    public async Task<IActionResult> OrganizationDetails(int id)
    {
        var org = await _context.Organizations
            .Include(o => o.Donations)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (org == null) return NotFound();
        return View(org);
    }

    // GET: Admin/Profile
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string email, string newPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        bool anyChange = false;

        try 
        {
            // 1. Force update Email/Username in DB
            if (!string.IsNullOrEmpty(email) && email != user.Email)
            {
                user.Email = email;
                user.NormalizedEmail = email.ToUpperInvariant();
                user.UserName = email;
                user.NormalizedUserName = email.ToUpperInvariant();
                anyChange = true;
            }

            // 2. Force reset password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, newPassword);
                anyChange = true;
            }

            if (anyChange)
            {
                // Direct DB Context Update to be 100% sure
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                // Refresh the security stamp AND the current sign-in session
                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                
                TempData["SuccessMessage"] = "Profile Updated & Session Refreshed! ✅";
            }
            else
            {
                TempData["SuccessMessage"] = "No changes detected.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "System Error: " + ex.Message;
        }

        return RedirectToAction(nameof(Profile));
    }

    public IActionResult Reports()
    {
        return View();
    }

    public async Task<IActionResult> Complaints()
    {
        var complaints = await _context.ContactMessages
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return View(complaints);
    }

    [HttpPost]
    public async Task<IActionResult> ResolveComplaint(int id)
    {
        var comp = await _context.ContactMessages.FindAsync(id);
        if (comp != null)
        {
            _context.ContactMessages.Remove(comp);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Complaint marked as Resolved and archived successfully!";
        }
        return RedirectToAction(nameof(Complaints));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComplaint(int id)
    {
        var comp = await _context.ContactMessages.FindAsync(id);
        if (comp != null)
        {
            _context.ContactMessages.Remove(comp);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Complaints));
    }

    public async Task<IActionResult> DonationsMonitoring()
    {
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Recipient)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
            
        return View(donations);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveDonation(int id)
    {
        var don = await _context.Donations.FindAsync(id);
        if(don != null)
        {
            _context.Donations.Remove(don);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(DonationsMonitoring));
    }

    // Food Tracking Page
    public async Task<IActionResult> FoodTracking()
    {
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Recipient)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return View(donations);
    }

    // Export CSV Report
    public async Task<IActionResult> ExportCsv()
    {
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Recipient)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ID,Food Item,Category,Quantity,Status,Donor,Recipient,Listed Date,Expiry Date");
        foreach (var d in donations)
        {
            sb.AppendLine($"{d.Id},\"{d.FoodItem}\",{d.Category},{d.Quantity},{d.Status},\"{d.Donor?.Name ?? "N/A"}\",\"{d.Recipient?.Name ?? "Unclaimed"}\",{d.CreatedAt:yyyy-MM-dd HH:mm},{d.ExpiryDate:yyyy-MM-dd HH:mm}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"ShareMeal_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }
}
