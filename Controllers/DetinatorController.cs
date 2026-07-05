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

        private async Task SetPrimulSalonIdAsync(string userId)
        {
            var detinator = await _context.Detinatori
                .Include(d => d.Saloane)
                .FirstOrDefaultAsync(d => d.UserId == userId);
            ViewBag.PrimulSalonId = detinator?.Saloane.FirstOrDefault()?.Id;
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
                programariAzi.AddRange(prog);
            }

            ViewBag.Detinator = detinator;
            ViewBag.ProgramariAsteptare = programariAzi
                .Count(p => p.Status == StatusProgramare.InAsteptare);
            ViewBag.ProgramariAzi = programariAzi
                .Count(p => p.DataOra.Date == DateTime.Today);
            ViewBag.TotalAfisate = programariAzi.Count;
            ViewBag.PrimulSalonId = detinator.Saloane.FirstOrDefault()?.Id;

            return View(programariAzi
                .Where(p => p.Status == StatusProgramare.InAsteptare)
                .OrderBy(p => p.DataOra)
                .ToList());
        }

        public async Task<IActionResult> Saloane()
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori
                .Include(d => d.Saloane)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (detinator == null) return RedirectToAction("Inregistrare");

            ViewBag.PrimulSalonId = detinator.Saloane.FirstOrDefault()?.Id;
            return View(detinator.Saloane.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> AdaugaSalon()
        {
            var userId = _userManager.GetUserId(User)!;
            await SetPrimulSalonIdAsync(userId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaSalon(Salon model, IFormFile? poza)
        {
            var userId = _userManager.GetUserId(User)!;
            var detinator = await _context.Detinatori.FirstOrDefaultAsync(d => d.UserId == userId);
            if (detinator == null) return RedirectToAction("Inregistrare");

            if (!ModelState.IsValid)
            {
                await SetPrimulSalonIdAsync(userId);
                return View(model);
            }

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

            await SetPrimulSalonIdAsync(userId);
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

            await SetPrimulSalonIdAsync(userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditeazaServiciu(int Id, int SalonId, string Nume,
            string Categorie, string Descriere, int DurataMinte, decimal Pret)
        {
            var userId = _userManager.GetUserId(User)!;
            var serviciu = await _context.Servicii
                .Include(s => s.Salon)
                .ThenInclude(sal => sal!.Detinator)
                .FirstOrDefaultAsync(s => s.Id == Id && s.Salon!.Detinator!.UserId == userId);

            if (serviciu == null) return Forbid();

            serviciu.Nume = Nume;
            serviciu.Categorie = Categorie;
            serviciu.Descriere = Descriere ?? "";
            serviciu.DurataMinte = DurataMinte;
            serviciu.Pret = Pret;

            await _context.SaveChangesAsync();
            TempData["Succes"] = "Serviciul a fost actualizat!";
            return RedirectToAction("Servicii", new { salonId = SalonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeServiciu(int id, int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var serviciu = await _context.Servicii
                .Include(s => s.Salon)
                .ThenInclude(sal => sal!.Detinator)
                .Include(s => s.Programari)
                .FirstOrDefaultAsync(s => s.Id == id && s.Salon!.Detinator!.UserId == userId);

            if (serviciu == null) return Forbid();

            if (serviciu.Programari.Any())
            {
                TempData["Eroare"] = "Nu poți șterge acest serviciu deoarece are programări asociate. Poți edita serviciul în loc să-l ștergi.";
                return RedirectToAction("Servicii", new { salonId });
            }

            _context.Servicii.Remove(serviciu);
            await _context.SaveChangesAsync();
            TempData["Succes"] = "Serviciul a fost șters.";
            return RedirectToAction("Servicii", new { salonId });
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

            var numarNeprezentari = programari
                .Where(p => p.Status == StatusProgramare.Neprezentat)
                .GroupBy(p => p.ClientId)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.Salon = salon;
            ViewBag.ClientiBlocati = clientiBlocati;
            ViewBag.NumarNeprezentari = numarNeprezentari;
            await SetPrimulSalonIdAsync(userId);
            return View(programari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizeazaStatus(int programareId, string status, int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            var statusEnum = status switch
            {
                "Acceptata" => StatusProgramare.Acceptata,
                "Respinsa"  => StatusProgramare.Respinsa,
                "Onorata"   => StatusProgramare.Onorata,
                _           => StatusProgramare.Respinsa
            };
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
            await SetPrimulSalonIdAsync(userId);
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
                .Select(sv => new { sv.Nume, Count = sv.Programari.Count })
                .OrderByDescending(sv => sv.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.ProgramariPeLuna = programariPeLuna;
            ViewBag.TopServicii = topServicii;
            ViewBag.Detinator = detinator;
            ViewBag.PrimulSalonId = detinator.Saloane.FirstOrDefault()?.Id;

            return View();
        }

        // PDF Tabelar — cu filtre perioada
public async Task<IActionResult> ExportPDFTabelar(int salonId, string perioada = "luna")
{
    var userId = _userManager.GetUserId(User)!;
    var salon = await _context.Saloane
        .Include(s => s.Detinator)
        .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

    if (salon == null) return Forbid();

    var acum = DateTime.Now;
    DateTime dataStart = perioada switch
    {
        "saptamana" => acum.AddDays(-7),
        "luna"      => new DateTime(acum.Year, acum.Month, 1),
        "an"        => new DateTime(acum.Year, 1, 1),
        _           => DateTime.MinValue // "toate"
    };

    var titluPerioada = perioada switch
    {
        "saptamana" => "Ultimele 7 zile",
        "luna"      => $"{acum:MMMM yyyy}",
        "an"        => $"Anul {acum.Year}",
        _           => "Toate programările"
    };

    var programari = await _context.Programari
        .Include(p => p.Client)
        .Include(p => p.Serviciu)
        .Where(p => p.Serviciu!.SalonId == salonId
                 && p.DataOra >= dataStart)
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
                col.Item().Text($"SalonBook — Raport {salon.Nume}").FontSize(18).Bold();
                col.Item().Text($"Perioada: {titluPerioada}").FontSize(11).FontColor("#3C3489");
                col.Item().Text($"Generat: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10).FontColor("#6b6b6b");
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
                        header.Cell().Background("#E1F5EE").Padding(6).Text(titlu).Bold().FontSize(9);
                });

                if (!programari.Any())
                {
                    table.Cell().ColumnSpan(5).Padding(20).AlignCenter()
                        .Text("Nu există programări în perioada selectată.").FontColor("#9b9b9b");
                }

                foreach (var p in programari)
                {
                    var culoare = p.Status switch
                    {
                        StatusProgramare.Acceptata => "#E1F5EE",
                        StatusProgramare.Onorata   => "#EAF3DE",
                        StatusProgramare.Respinsa  => "#FCEBEB",
                        StatusProgramare.Anulata   => "#FCEBEB",
                        _                          => "#FAEEDA"
                    };
                    var statusText = p.Status switch
                    {
                        StatusProgramare.InAsteptare => "In asteptare",
                        StatusProgramare.Acceptata   => "Acceptata",
                        StatusProgramare.Onorata     => "Onorata",
                        StatusProgramare.Respinsa    => "Respinsa",
                        StatusProgramare.Anulata     => "Anulata",
                        _                            => p.Status.ToString()
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

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Pagina "); x.CurrentPageNumber(); x.Span(" din "); x.TotalPages();
            });
        });
    });

    return File(pdf.GeneratePdf(), "application/pdf",
        $"raport-{perioada}-{salon.Nume}-{DateTime.Now:yyyy-MM-dd}.pdf");
}

// PDF Program pe Zi — doar de azi inainte
public async Task<IActionResult> ExportPDFZilnic(int salonId)
{
    var userId = _userManager.GetUserId(User)!;
    var salon = await _context.Saloane
        .Include(s => s.Detinator)
        .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

    if (salon == null) return Forbid();

    var programari = await _context.Programari
        .Include(p => p.Client)
        .Include(p => p.Serviciu)
        .Where(p => p.Serviciu!.SalonId == salonId
                 && p.DataOra.Date >= DateTime.Today  // doar de azi inainte
                 && p.Status != StatusProgramare.Respinsa
                 && p.Status != StatusProgramare.Anulata)
        .OrderBy(p => p.DataOra)
        .ToListAsync();

    var peZile = programari.GroupBy(p => p.DataOra.Date).OrderBy(g => g.Key).ToList();

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
                col.Item().Text($"Program zilnic — {salon.Nume}").FontSize(18).Bold();
                col.Item().Text($"De la: {DateTime.Today:dd MMM yyyy}").FontSize(11).FontColor("#3C3489");
                col.Item().Text($"Generat: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10).FontColor("#6b6b6b");
                col.Item().PaddingTop(8).LineHorizontal(0.5f);
            });

            page.Content().PaddingTop(16).Column(col =>
            {
                foreach (var zi in peZile)
                {
                    col.Item().PaddingTop(12)
                        .Text(zi.Key.ToString("dddd, dd MMM yyyy"))
                        .FontSize(13).Bold();
                    col.Item().PaddingBottom(6).LineHorizontal(0.5f);

                    foreach (var p in zi.OrderBy(p => p.DataOra))
                    {
                        var culoare = p.Status switch
                        {
                            StatusProgramare.Onorata   => "#EAF3DE",
                            StatusProgramare.Acceptata => "#E1F5EE",
                            _                          => "#FAEEDA"
                        };

                        col.Item().PaddingBottom(4).Row(row =>
                        {
                            row.ConstantItem(50).Background(culoare)
                                .Padding(6).AlignCenter()
                                .Text(p.DataOra.ToString("HH:mm"))
                                .FontSize(11).Bold();

                            row.RelativeItem().BorderBottom(0.5f).BorderColor("#e5e5e5")
                                .Padding(6).Column(c =>
                                {
                                    c.Item().Text($"{p.Client?.Prenume} {p.Client?.Nume}")
                                        .FontSize(11).Bold();
                                    c.Item().Text($"{p.Serviciu?.Nume} · {p.Serviciu?.DurataMinte} min · {p.Serviciu?.Pret} lei")
                                        .FontSize(10).FontColor("#6b6b6b");
                                });

                            row.ConstantItem(70).AlignRight().Padding(6)
                                .Text(p.Status == StatusProgramare.Onorata ? "✓ Onorat" :
                                      p.Status == StatusProgramare.Acceptata ? "Acceptat" : "Asteptare")
                                .FontSize(9).FontColor(
                                    p.Status == StatusProgramare.Onorata ? "#3B6D11" :
                                    p.Status == StatusProgramare.Acceptata ? "#0F6E56" : "#633806");
                        });
                    }

                    col.Item().PaddingBottom(8)
                        .Text($"Total zi: {zi.Count()} programări · {zi.Sum(p => p.Serviciu?.DurataMinte ?? 0)} minute")
                        .FontSize(9).FontColor("#9b9b9b");
                }

                if (!peZile.Any())
                    col.Item().PaddingTop(20).AlignCenter()
                        .Text("Nu există programări viitoare active.").FontColor("#9b9b9b");
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Pagina "); x.CurrentPageNumber(); x.Span(" din "); x.TotalPages();
            });
        });
    });

    return File(pdf.GeneratePdf(), "application/pdf",
        $"program-zilnic-{salon.Nume}-{DateTime.Now:yyyy-MM-dd}.pdf");
}

        // Marcheaza ca Onorata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcheazaOnorata(int programareId, int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _programareService.ActualizeazaStatusAsync(
                programareId, StatusProgramare.Onorata, userId);
            TempData["Succes"] = "Programarea a fost marcată ca onorată.";
            return RedirectToAction("Programari", new { salonId });
        }

        // Marcheaza ca Neprezentat (client nu a venit, dar nici nu a anulat)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcheazaNeprezentat(int programareId, int salonId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _programareService.ActualizeazaStatusAsync(
                programareId, StatusProgramare.Neprezentat, userId);
            TempData["Succes"] = "Programarea a fost marcată ca neprezentare.";
            return RedirectToAction("Programari", new { salonId });
        }

        // Adauga perioada blocata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaPerioadaBlocata(
            int salonId, DateTime dataStart, DateTime dataSfarsit, string? motiv)
        {
            var userId = _userManager.GetUserId(User)!;
            var salon = await _context.Saloane
                .Include(s => s.Detinator)
                .FirstOrDefaultAsync(s => s.Id == salonId && s.Detinator!.UserId == userId);

            if (salon == null) return Forbid();

            _context.PerioadeBlockate.Add(new PerioadaBlocata
            {
                SalonId = salonId,
                DataStart = dataStart,
                DataSfarsit = dataSfarsit,
                Motiv = motiv
            });
            await _context.SaveChangesAsync();

            TempData["Succes"] = "Perioada a fost blocată.";
            return RedirectToAction("Calendar", new { salonId });
        }

        // Sterge perioada blocata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergePerioadaBlocata(int perioadaId, int salonId)
        {
            var perioada = await _context.PerioadeBlockate.FindAsync(perioadaId);
            if (perioada != null)
            {
                _context.PerioadeBlockate.Remove(perioada);
                await _context.SaveChangesAsync();
            }
            TempData["Succes"] = "Perioada a fost deblocată.";
            return RedirectToAction("Calendar", new { salonId });
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

            var perioade = await _context.PerioadeBlockate
                .Where(pb => pb.SalonId == salonId)
                .ToListAsync();

            ViewBag.Salon = salon;
            ViewBag.PrimulSalonId = salonId;
            ViewBag.PerioadeBlockateJson = System.Text.Json.JsonSerializer.Serialize(
                perioade.Select(pb => new
                {
                    id = pb.Id,
                    dataStart = pb.DataStart.ToString("yyyy-MM-dd"),
                    dataSfarsit = pb.DataSfarsit.ToString("yyyy-MM-dd"),
                    motiv = pb.Motiv ?? "Indisponibil"
                }));
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