using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;
using SalonBook.Services;

namespace SalonBook.Controllers
{
    [Authorize(Roles = "Client,Detinator")]
    public class ClientController : Controller
    {
        private readonly IProgramareService _programareService;
        private readonly INotificareService _notificareService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ClientController(IProgramareService programareService,
                                 INotificareService notificareService,
                                 UserManager<ApplicationUser> userManager,
                                 ApplicationDbContext context)
        {
            _programareService = programareService;
            _notificareService = notificareService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var programari = await _programareService.GetProgramariClientAsync(userId);
            return View(programari);
        }

        public async Task<IActionResult> Notificari()
        {
            var userId = _userManager.GetUserId(User)!;
            var notificari = await _notificareService.GetNotificariAsync(userId);
            return View(notificari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcheazaCitita(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _notificareService.MarcheazaCititaAsync(id, userId);
            return RedirectToAction("Notificari");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnuleazaProgramare(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var programare = await _context.Programari
                .FirstOrDefaultAsync(p => p.Id == id && p.ClientId == userId);

            if (programare != null && programare.Status == StatusProgramare.InAsteptare)
            {
                programare.Status = StatusProgramare.Anulata;
                await _context.SaveChangesAsync();
                TempData["Succes"] = "Programarea a fost anulată.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(string Prenume, string Nume,
            string Telefon, string Adresa)
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.Prenume = Prenume;
                user.Nume = Nume;
                user.PhoneNumber = Telefon;
                user.Adresa = Adresa;
                await _userManager.UpdateAsync(user);
                TempData["Succes"] = "Profilul a fost actualizat!";
            }

            return RedirectToAction("Profil");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaRecenzie(int salonId, int nota, string? comentariu)
        {
            var userId = _userManager.GetUserId(User)!;

            var areProgramareAcceptata = await _context.Programari
                .AnyAsync(p => p.ClientId == userId
                    && p.Serviciu!.SalonId == salonId
                    && p.Status == StatusProgramare.Acceptata);

            if (!areProgramareAcceptata)
            {
                TempData["Eroare"] = "Poți lăsa o recenzie doar după o programare acceptată.";
                return RedirectToAction("Index", "Salon", new { id = salonId });
            }

            var recenzieExistenta = await _context.Recenzii
                .AnyAsync(r => r.SalonId == salonId && r.ClientId == userId);

            if (recenzieExistenta)
            {
                TempData["Eroare"] = "Ai lăsat deja o recenzie pentru acest salon.";
                return RedirectToAction("Index", "Salon", new { id = salonId });
            }

            var recenzie = new Recenzie
            {
                SalonId = salonId,
                ClientId = userId,
                Nota = nota,
                Comentariu = comentariu,
                DataCreare = DateTime.Now
            };

            _context.Recenzii.Add(recenzie);
            await _context.SaveChangesAsync();

            TempData["Succes"] = "Recenzia ta a fost adăugată!";
            return RedirectToAction("Index", "Salon", new { id = salonId });
        }
    }
}