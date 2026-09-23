using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShareMeal.Web.Data;
using ShareMeal.Web.Models;

namespace ShareMeal.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Restaurant"))
                            return RedirectToAction("Dashboard", "Restaurant");
                        if (await _userManager.IsInRoleAsync(user, "Charity"))
                            return RedirectToAction("Dashboard", "Charity");
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                            return RedirectToAction("Dashboard", "Admin");
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Only allow Restaurant or Charity role from public registration
                var allowedRoles = new[] { "Restaurant", "Charity" };
                if (!allowedRoles.Contains(model.Role))
                {
                    ModelState.AddModelError("Role", "Please select a valid account type.");
                    return View(model);
                }

                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Create Organization record linked to this user
                    var orgName = model.Email.Split('@')[0];
                    orgName = char.ToUpper(orgName[0]) + orgName.Substring(1) + (model.Role == "Restaurant" ? " Restaurant" : " Foundation");

                    var org = new Organization
                    {
                        Name = orgName,
                        Type = model.Role == "Restaurant" ? OrganizationType.Restaurant : OrganizationType.Charity,
                        ContactEmail = model.Email,
                        OwnerId = user.Id,
                        Status = OrganizationStatus.Verified,
                        Address = "Islamabad, Pakistan",
                        Phone = "+92 300 1234567"
                    };
                    _context.Organizations.Add(org);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                        ? Redirect(returnUrl)
                        : (model.Role == "Restaurant" 
                            ? RedirectToAction("Dashboard", "Restaurant") 
                            : RedirectToAction("Dashboard", "Charity"));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ExternalLogin(string provider = "Google", string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var demoEmail = provider?.ToLower() == "facebook" ? "khulfanchoudhary@gmail.com" : "mkhulfan9@gmail.com";
            var user = await _userManager.FindByEmailAsync(demoEmail);
            if (user == null)
            {
                user = _userManager.Users.FirstOrDefault();
            }

            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) 
                    ? Redirect(returnUrl) 
                    : RedirectToAction("Index", "Home");
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
