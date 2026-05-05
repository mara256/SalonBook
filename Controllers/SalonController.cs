using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;

namespace SalonBook.Controllers
{
    public class SalonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalonController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            var salon = await _context.Saloane
                .Include(s => s.Servicii)
                .Include(s => s.Detinator)
                .Include(s => s.Recenzii)
                    .ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salon == null) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                var esteBlocat = await _context.ClientiBlocati
                    .AnyAsync(cb => cb.SalonId == id && cb.ClientId == userId);

                if (esteBlocat)
                {
                    TempData["Eroare"] = "Nu poți accesa acest salon.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.PoateLasaRecenzie = await _context.Programari
                    .AnyAsync(p => p.ClientId == userId
                        && p.Serviciu!.SalonId == id
                        && p.Status == StatusProgramare.Acceptata);

                ViewBag.ARecenzat = await _context.Recenzii
                    .AnyAsync(r => r.SalonId == id && r.ClientId == userId);
            }

            var recenzii = salon.Recenzii.OrderByDescending(r => r.DataCreare).ToList();
            ViewBag.Recenzii = recenzii;
            ViewBag.NotaMedie = recenzii.Any()
                ? recenzii.Average(r => r.Nota).ToString("F1")
                : null;

            return View(salon);
        }
    }
}