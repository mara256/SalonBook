using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;
using SalonBook.Services;

namespace SalonBook.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProgramareService _programareService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context,
                                  IProgramareService programareService,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _programareService = programareService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int salonId)
        {
            var salon = await _context.Saloane
                .Include(s => s.Servicii)
                .FirstOrDefaultAsync(s => s.Id == salonId);

            if (salon == null) return NotFound();
            return View(salon);
        }

        public async Task<IActionResult> SelectData(int serviciuId)
        {
            var serviciu = await _context.Servicii
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == serviciuId);

            if (serviciu == null) return NotFound();

            ViewBag.ServiciuId = serviciuId;
            ViewBag.Serviciu = serviciu;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OreDisponibile(int serviciuId, string data)
        {
            if (!DateTime.TryParse(data, out var dataSelectata))
                return BadRequest();

            var ore = await _programareService.GetOreDisponibileAsync(serviciuId, dataSelectata);
            return Json(ore.Select(o => o.ToString("HH:mm")));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirma(int serviciuId, string dataOra)
        {
            if (!DateTime.TryParse(dataOra, out var dt))
            {
                TempData["Eroare"] = "Data sau ora invalida.";
                return RedirectToAction("SelectData", new { serviciuId });
            }

            var userId = _userManager.GetUserId(User)!;

            try
            {
                var programare = await _programareService.CreazaProgramareAsync(userId, serviciuId, dt);
                TempData["Succes"] = "Programarea a fost trimisa! Vei fi notificat cand este confirmata.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Eroare"] = ex.Message;
                return RedirectToAction("SelectData", new { serviciuId });
            }

            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            else if (User.IsInRole("Detinator") && !User.IsInRole("Client"))
                return RedirectToAction("Index", "Detinator");
            else
                return RedirectToAction("Index", "Client");
        }
    }
}