using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;
using SalonBook.ViewModels;

namespace SalonBook.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Parola, model.TineLogat, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin");
                    else if (await _userManager.IsInRoleAsync(user, "Detinator"))
                        return RedirectToAction("Index", "Detinator");
                    else
                        return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Client");
                }
            }

            ModelState.AddModelError(string.Empty, "Email sau parolă incorectă.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nume = model.Nume,
                Prenume = model.Prenume,
                PhoneNumber = model.Telefon,
                Adresa = model.Adresa,
                Rol = model.TipCont
            };

            var result = await _userManager.CreateAsync(user, model.Parola);
            if (result.Succeeded)
            {
                if (model.TipCont == "Detinator" && !string.IsNullOrEmpty(model.DenumireFirma))
                {
                    await _userManager.AddToRoleAsync(user, "Detinator");

                    var detinator = new Detinator
                    {
                        UserId = user.Id,
                        DenumireFirma = model.DenumireFirma!,
                        AdresaFirma = model.AdresaFirma ?? "",
                        Telefon = model.TelefonFirma ?? model.Telefon ?? ""
                    };
                    _context.Detinatori.Add(detinator);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Detinator");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Client");
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> NotificariCount()
        {
            var userId = _userManager.GetUserId(User)!;
            var count = await _context.Notificari
                .CountAsync(n => n.UserId == userId && !n.EsteCitita);
            return Json(count);
        }

        public IActionResult AccessDenied() => View();

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}