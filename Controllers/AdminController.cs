using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SalonBook.Data;
using SalonBook.Models;
namespace SalonBook.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClienti = await _userManager.GetUsersInRoleAsync("Client");
            ViewBag.TotalDetinatori = (await _userManager.GetUsersInRoleAsync("Detinator")).Count;
            ViewBag.TotalSaloane = await _context.Saloane.CountAsync(s => s.Status == StatusSalon.Aprobat);
            ViewBag.TotalProgramari = await _context.Programari.CountAsync();
            ViewBag.SaloanePending = await _context.Saloane.CountAsync(s => s.Status == StatusSalon.InAsteptare);
            return View();
        }

        public async Task<IActionResult> Utilizatori()
        {
            var utilizatori = await _context.Users.ToListAsync();
            return View(utilizatori);
        }

        public async Task<IActionResult> Detinatori()
        {
            var detinatori = await _context.Detinatori
                .Include(d => d.User)
                .Include(d => d.Saloane)
                .ToListAsync();
            return View(detinatori);
        }

        public async Task<IActionResult> Saloane()
        {
            var saloane = await _context.Saloane
                .Include(s => s.Detinator)
                    .ThenInclude(d => d!.User)
                .Include(s => s.Servicii)
                .OrderBy(s => s.Status)
                .ToListAsync();
            return View(saloane);
        }

        public async Task<IActionResult> Programari()
        {
            var programari = await _context.Programari
                .Include(p => p.Client)
                .Include(p => p.Serviciu)
                    .ThenInclude(s => s!.Salon)
                .OrderByDescending(p => p.DataCreare)
                .ToListAsync();
            return View(programari);
        }

        public async Task<IActionResult> Statistici()
        {
            var acum = DateTime.Now;
            var programariPeLuna = new List<object>();

            for (int i = 5; i >= 0; i--)
            {
                var luna = acum.AddMonths(-i);
                var count = await _context.Programari
                    .CountAsync(p => p.DataOra.Month == luna.Month
                                  && p.DataOra.Year == luna.Year);
                programariPeLuna.Add(new {
                    luna = luna.ToString("MMM yyyy"),
                    count
                });
            }

            ViewBag.InAsteptare = await _context.Programari
                .CountAsync(p => p.Status == StatusProgramare.InAsteptare);
            ViewBag.Acceptate = await _context.Programari
                .CountAsync(p => p.Status == StatusProgramare.Acceptata);
            ViewBag.Respinse = await _context.Programari
                .CountAsync(p => p.Status == StatusProgramare.Respinsa);
            ViewBag.Anulate = await _context.Programari
                .CountAsync(p => p.Status == StatusProgramare.Anulata);

            var topSaloane = await _context.Saloane
                .Include(s => s.Servicii)
                    .ThenInclude(sv => sv.Programari)
                .Select(s => new {
                    s.Nume,
                    Count = s.Servicii.Sum(sv => sv.Programari.Count)
                })
                .OrderByDescending(s => s.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.ProgramariPeLuna = programariPeLuna;
            ViewBag.TopSaloane = topSaloane;

            return View();
        }

        public async Task<IActionResult> ExportPDF()
        {
            var programari = await _context.Programari
                .Include(p => p.Client)
                .Include(p => p.Serviciu)
                    .ThenInclude(s => s!.Salon)
                .OrderByDescending(p => p.DataOra)
                .ToListAsync();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("SalonBook — Raport Programări")
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
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            foreach (var titlu in new[] { "Client", "Salon", "Serviciu", "Data", "Preț", "Status" })
                            {
                                header.Cell().Background("#EEEDFE").Padding(6)
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
                                .Text(p.Serviciu?.Salon?.Nume ?? "").FontSize(9);
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
                $"raport-programari-{DateTime.Now:yyyy-MM-dd}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobaSalon(int salonId)
        {
            var salon = await _context.Saloane.FindAsync(salonId);
            if (salon != null)
            {
                salon.Status = StatusSalon.Aprobat;
                salon.EsteActiv = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Saloane");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespingeSalon(int salonId)
        {
            var salon = await _context.Saloane.FindAsync(salonId);
            if (salon != null)
            {
                salon.Status = StatusSalon.Respins;
                salon.EsteActiv = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Saloane");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSalon(int salonId)
        {
            var salon = await _context.Saloane.FindAsync(salonId);
            if (salon != null)
            {
                salon.EsteActiv = !salon.EsteActiv;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Saloane");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeUtilizator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

                try
                {
                    var notificari = _context.Notificari.Where(n => n.UserId == userId);
                    _context.Notificari.RemoveRange(notificari);
                    await _context.SaveChangesAsync();

                    var clientiBlocatiAsClient = _context.ClientiBlocati
                        .Where(cb => cb.ClientId == userId);
                    _context.ClientiBlocati.RemoveRange(clientiBlocatiAsClient);
                    await _context.SaveChangesAsync();

                    var programari = _context.Programari.Where(p => p.ClientId == userId);
                    _context.Programari.RemoveRange(programari);
                    await _context.SaveChangesAsync();

                    var detinator = await _context.Detinatori
                        .Include(d => d.Saloane)
                            .ThenInclude(s => s.Servicii)
                        .FirstOrDefaultAsync(d => d.UserId == userId);

                    if (detinator != null)
                    {
                        foreach (var salon in detinator.Saloane)
                        {
                            var blocatiSalon = _context.ClientiBlocati
                                .Where(cb => cb.SalonId == salon.Id);
                            _context.ClientiBlocati.RemoveRange(blocatiSalon);
                            await _context.SaveChangesAsync();

                            foreach (var serviciu in salon.Servicii)
                            {
                                var progIds = _context.Programari
                                    .Where(p => p.ServiciuId == serviciu.Id)
                                    .Select(p => p.Id);
                                var notifProg = _context.Notificari
                                    .Where(n => n.ProgramareId != null &&
                                           progIds.Contains(n.ProgramareId.Value));
                                _context.Notificari.RemoveRange(notifProg);
                                await _context.SaveChangesAsync();

                                var progSalon = _context.Programari
                                    .Where(p => p.ServiciuId == serviciu.Id);
                                _context.Programari.RemoveRange(progSalon);
                                await _context.SaveChangesAsync();
                            }

                            _context.Servicii.RemoveRange(salon.Servicii);
                            await _context.SaveChangesAsync();
                        }

                        _context.Saloane.RemoveRange(detinator.Saloane);
                        await _context.SaveChangesAsync();

                        _context.Detinatori.Remove(detinator);
                        await _context.SaveChangesAsync();
                    }

                    var roluri = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, roluri);
                    await _userManager.DeleteAsync(user);
                }
                finally
                {
                    await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
                }
            }
            return RedirectToAction("Utilizatori");
        }
    }
}