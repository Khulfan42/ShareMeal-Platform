using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;
using System.Diagnostics;

namespace ShareMeal.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new HomeStatsViewModel
        {
            TotalMeals = await _context.Donations.CountAsync(),
            ActiveDonations = await _context.Donations.CountAsync(d => d.Status == DonationStatus.Available),
            Organizations = await _context.Organizations.CountAsync()
        };
        return View(stats);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccountStatus(OrganizationStatus status)
    {
        ViewBag.Status = status;
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult HowItWorks()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Contact(ContactMessage contactForm)
    {
        // Prevent silent validation failures on implicit properties
        ModelState.Remove("Id");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("IsResolved");

        if (ModelState.IsValid)
        {
            _context.ContactMessages.Add(contactForm);
            await _context.SaveChangesAsync();
            return RedirectToAction("Contact", new { success = true });
        }
        return View(contactForm);
    }

    public IActionResult Developer()
    {
        return View();
    }

    public IActionResult Design()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
