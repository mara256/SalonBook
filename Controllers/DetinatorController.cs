using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;
using SalonBook.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SalonBook.Controllers
{
    [Authorize(Roles = "Detinator")]
    public class DetinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProgramareService _programareService;
        private readonly INotificareService _notificareService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetinatorController(ApplicationDbContext context,
                                    IProgramareService programareService,
                                    INotificareService notificareService,
                                    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _programareService = programareService;
            _notificareService = notificareService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori
                .Include(d => d.Saloane)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (detinator == null) return RedirectToAction("Inregistrare");

            var programariAzi = new List<Programare>();
            foreach (var salon in detinator.Saloane)
            {
                var prog = await _programareService.GetProgramariSalonAsync(salon.Id);
                programariAzi.AddRange(prog.Where(p => p.DataOra.Date == DateTime.Today));
            }

            ViewBag.Detinator = detinator;
            ViewBag.ProgramariAsteptare = programariAzi.Count(p => p.Status == StatusProgramare.InAsteptare);
            ViewBag.ProgramariAzi = programariAzi.Count;

            return View(programariAzi.OrderBy(p => p.DataOra).ToList());
        }

        public async Task<IActionResult> Saloane()
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori
                .Include(d => d.Saloane)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (detinator == null) return RedirectToAction("Inregistrare");
            return View(detinator.Saloane.ToList());
        }

        [HttpGet]
        public IActionResult AdaugaSalon() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaSalon(Salon model, IFormFile? poza)
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori.FirstOrDefaultAsync(d => d.UserId == userId);
            if (detinator == null) return RedirectToAction("Inregistrare");

            if (!ModelState.IsValid) return View(model);

            if (poza != null && poza.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(poza.FileName)}";
                var path = Path.Combine("wwwroot/images/saloane", fileName);
                Directory.CreateDirectory("wwwroot/images/saloane");
                using var stream = new FileStream(path, FileMode.Create);
                await poza.CopyToAsync(stream);
                model.PozaUrl = $"/images/saloane/{fileName}";
            }

            model.DetinatorId = detinator.Id;
            _context.Saloane.Add(model);
            await _context.SaveChangesAsync();

            TempData["Succes"] = "Salonul a fost adaugat cu succes!";
            return RedirectToAction("Saloane");
        }

        [HttpGet]
        public async Task<IActionResult> EditeazaSalon(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == id && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();
            return View(salon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditeazaSalon(int Id, string Nume, string Adresa,
            string Descriere, double? Latitudine, double? Longitudine,
            TimeSpan OraDeschierii, TimeSpan OraInchiderii, IFormFile? poza)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == Id && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();

            salon.Nume = Nume;
            salon.Adresa = Adresa;
            salon.Descriere = Descriere ?? "";
            salon.Latitudine = Latitudine;
            salon.Longitudine = Longitudine;
            salon.OraDeschierii = OraDeschierii;
            salon.OraInchiderii = OraInchiderii;

            if (poza != null && poza.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(poza.FileName)}";
                var path = Path.Combine("wwwroot/images/saloane", fileName);
                Directory.CreateDirectory("wwwroot/images/saloane");
                using var stream = new FileStream(path, FileMode.Create);
                await poza.CopyToAsync(stream);
                salon.PozaUrl = $"/images/saloane/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Succes"] = "Salonul a fost actualizat!";
            return RedirectToAction("Saloane");
        }

        public async Task<IActionResult> Servicii(int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Servicii)
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();
            return View(salon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaServiciu(int SalonId, string Nume,
            string Categorie, string Descriere, int DurataMinte, decimal Pret)
        {
            var serviciu = new Serviciu
            {
                SalonId = SalonId,
                Nume = Nume,
                Categorie = Categorie,
                Descriere = Descriere ?? "",
                DurataMinte = DurataMinte,
                Pret = Pret
            };

            _context.Servicii.Add(serviciu);
            await _context.SaveChangesAsync();
            TempData["Succes"] = "Serviciul a fost adaugat!";
            return RedirectToAction("Servicii", new { salonId = SalonId });
        }

        public async Task<IActionResult> Programari(int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();

            var programari = await _programareService.GetProgramariSalonAsync(salonId);

            var clientiBlocati = await _context.ClientiBlocati
                .Where(cb => cb.SalonId == salonId)
                .Select(cb => cb.ClientId)
                .ToListAsync();

            ViewBag.Salon = salon;
            ViewBag.ClientiBlocati = clientiBlocati;
            return View(programari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizeazaStatus(int programareId, string status, int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var statusEnum = status == "Acceptata" ? StatusProgramare.Acceptata : StatusProgramare.Respinsa;
            await _programareService.ActualizeazaStatusAsync(programareId, statusEnum, userId);
            return RedirectToAction("Programari", new { salonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlocheazaClient(string clientId, int salonId, string? motiv)
        {
            var userId = _userManager.GetUserId(User)!;

            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();

            var exista = await _context.ClientiBlocati
                .AnyAsync(cb => cb.SalonId == salonId && cb.ClientId == clientId);

            if (!exista)
            {
                _context.ClientiBlocati.Add(new ClientBlocat
                {
                    SalonId = salonId,
                    ClientId = clientId,
                    Motiv = motiv,
                    DataBlocare = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Succes"] = "Clientul a fost blocat cu succes.";
            }
            else
            {
                TempData["Eroare"] = "Clientul este deja blocat.";
            }

            return RedirectToAction("Programari", new { salonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DezblocheazaClient(string clientId, int salonId)
        {
            var inregistrare = await _context.ClientiBlocati
                .FirstOrDefaultAsync(cb => cb.SalonId == salonId && cb.ClientId == clientId);

            if (inregistrare != null)
            {
                _context.ClientiBlocati.Remove(inregistrare);
                await _context.SaveChangesAsync();
                TempData["Succes"] = "Clientul a fost deblocat.";
            }

            return RedirectToAction("Programari", new { salonId });
        }

        public async Task<IActionResult> Notificari()
        {
            var userId = _userManager.GetUserId(User)!;
            var notificari = await _notificareService.GetNotificariAsync(userId);
            return View(notificari);
        }

        public async Task<IActionResult> Statistici()
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori
                .Include(d => d.Saloane)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (detinator == null) return RedirectToAction("Inregistrare");

            var salonIds = detinator.Saloane.Select(s => s.Id).ToList();

            var acum = DateTime.Now;
            var programariPeLuna = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var luna = acum.AddMonths(-i);
                var count = await _context.Programari
                    .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId)
                                && p.DataOra.Month == luna.Month
                                && p.DataOra.Year == luna.Year);
                programariPeLuna.Add(new { luna = luna.ToString("MMM yyyy"), count });
            }

            ViewBag.InAsteptare = await _context.Programari
                .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId)
                            && p.Status == StatusProgramare.InAsteptare);
            ViewBag.Acceptate = await _context.Programari
                .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId)
                            && p.Status == StatusProgramare.Acceptata);
            ViewBag.Respinse = await _context.Programari
                .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId)
                            && p.Status == StatusProgramare.Respinsa);
            ViewBag.Anulate = await _context.Programari
                .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId)
                            && p.Status == StatusProgramare.Anulata);

            ViewBag.TotalProgramari = await _context.Programari
                .CountAsync(p => salonIds.Contains(p.Serviciu!.SalonId));

            ViewBag.TotalClienti = await _context.Programari
                .Where(p => salonIds.Contains(p.Serviciu!.SalonId))
                .Select(p => p.ClientId)
                .Distinct()
                .CountAsync();

            var topServicii = await _context.Servicii
                .Where(sv => salonIds.Contains(sv.SalonId))
                .Select(sv => new {
                    sv.Nume,
                    Count = sv.Programari.Count
                })
                .OrderByDescending(sv => sv.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.ProgramariPeLuna = programariPeLuna;
            ViewBag.TopServicii = topServicii;
            ViewBag.Detinator = detinator;

            return View();
        }
        
        public async Task<IActionResult> ExportPDF(int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();

            var programari = await _context.Programari
                .Include(p => p.Client)
                .Include(p => p.Serviciu)
                .Where(p => p.Serviciu!.SalonId == salonId)
                .OrderByDescending(p => p.DataOra)
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"SalonBook — Raport {salon.Nume}")
                            .FontSize(18).Bold();
                        col.Item().Text($"Generat: {DateTime.Now:dd MMM yyyy HH:mm}")
                            .FontSize(10).FontColor("#6b6b6b");
                        col.Item().PaddingTop(8).LineHorizontal(0.5f);
                    });

                    page.Content().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            foreach (var titlu in new[] { "Client", "Serviciu", "Data", "Preț", "Status" })
                            {
                                header.Cell().Background("#E1F5EE").Padding(6)
                                    .Text(titlu).Bold().FontSize(9);
                            }
                        });

                        foreach (var p in programari)
                        {
                            var culoare = p.Status switch
                            {
                                StatusProgramare.Acceptata => "#E1F5EE",
                                StatusProgramare.Respinsa => "#FCEBEB",
                                StatusProgramare.Anulata => "#FCEBEB",
                                _ => "#FAEEDA"
                            };

                            var statusText = p.Status switch
                            {
                                StatusProgramare.InAsteptare => "In asteptare",
                                StatusProgramare.Acceptata => "Acceptata",
                                StatusProgramare.Respinsa => "Respinsa",
                                StatusProgramare.Anulata => "Anulata",
                                _ => p.Status.ToString()
                            };

                            table.Cell().BorderBottom(0.5f).BorderColor("#e5e5e5").Padding(6)
                                .Text($"{p.Client?.Prenume} {p.Client?.Nume}").FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e5e5e5").Padding(6)
                                .Text(p.Serviciu?.Nume ?? "").FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e5e5e5").Padding(6)
                                .Text(p.DataOra.ToString("dd MMM yyyy HH:mm")).FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e5e5e5").Padding(6)
                                .Text($"{p.Serviciu?.Pret} lei").FontSize(9);
                            table.Cell().BorderBottom(0.5f).BorderColor("#e5e5e5").Background(culoare).Padding(6)
                                .Text(statusText).FontSize(9);
                        }
                    });

                    page.Footer().AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Pagina ");
                            x.CurrentPageNumber();
                            x.Span(" din ");
                            x.TotalPages();
                        });
                });
            });

            var bytes = pdf.GeneratePdf();
            return File(bytes, "application/pdf",
                $"raport-{salon.Nume}-{DateTime.Now:yyyy-MM-dd}.pdf");
        }
                
                public async Task<IActionResult> Calendar(int salonId)
                {
                    var userId = _userManager.GetUserId(User)!;
                    var salon = await _context.Saloane
                        .Include(s => s.Detinator)
                        .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

                    if (salon == null) return Forbid();

                    var programari = await _context.Programari
                        .Include(p => p.Client)
                        .Include(p => p.Serviciu)
                        .Where(p => p.Serviciu!.SalonId == salonId)
                        .OrderBy(p => p.DataOra)
                        .ToListAsync();

                    ViewBag.Salon = salon;
                    ViewBag.ProgramariJson = System.Text.Json.JsonSerializer.Serialize(
                        programari.Select(p => new
                        {
                            id = p.Id,
                            titlu = $"{p.Serviciu?.Nume} — {p.Client?.Prenume} {p.Client?.Nume}",
                            dataOra = p.DataOra.ToString("yyyy-MM-ddTHH:mm"),
                            durata = p.Serviciu?.DurataMinte ?? 30,
                            status = p.Status.ToString(),
                            client = $"{p.Client?.Prenume} {p.Client?.Nume}",
                            serviciu = p.Serviciu?.Nume,
                            pret = p.Serviciu?.Pret
                        }));

                    return View();
                }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Inregistrare() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inregistrare(Detinator model, bool SiClient = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            model.UserId = userId;
            _context.Detinatori.Add(model);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!SiClient)
                    await _userManager.RemoveFromRoleAsync(user, "Client");

                await _userManager.AddToRoleAsync(user, "Detinator");
                user.Rol = "Detinator";
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }
    }
}