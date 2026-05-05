using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;

namespace SalonBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? categorie, string? cautare,
            string? pretMax, string? sortare, string? oras)
        {
            var saloane = _context.Saloane
                .Include(s => s.Servicii)
                .Include(s => s.Recenzii)
                .Where(s => s.EsteActiv && s.Status == StatusSalon.Aprobat);

            if (!string.IsNullOrEmpty(categorie))
                saloane = saloane.Where(s => s.Servicii.Any(sv => sv.Categorie == categorie));

            if (!string.IsNullOrEmpty(cautare))
                saloane = saloane.Where(s => s.Nume.Contains(cautare) || s.Adresa.Contains(cautare));

            if (!string.IsNullOrEmpty(pretMax) && decimal.TryParse(pretMax, out var pret))
                saloane = saloane.Where(s => s.Servicii.Any(sv => sv.Pret <= pret));

            if (!string.IsNullOrEmpty(oras))
                saloane = saloane.Where(s => s.Adresa.Contains(oras));

            if (sortare == "nume")
                saloane = saloane.OrderBy(s => s.Nume);

            ViewBag.Categorie = categorie;
            ViewBag.Cautare = cautare;
            ViewBag.PretMax = pretMax;
            ViewBag.Sortare = sortare;
            ViewBag.Oras = oras;

            var orase = await _context.Saloane
                .Where(s => s.EsteActiv && s.Status == StatusSalon.Aprobat)
                .Select(s => s.Adresa)
                .ToListAsync();

            ViewBag.Orase = orase
                .Select(a => a.Split(',').LastOrDefault()?.Trim() ?? "")
                .Where(o => !string.IsNullOrEmpty(o))
                .Distinct()
                .OrderBy(o => o)
                .ToList();

            var lista = await saloane.ToListAsync();

            lista = sortare switch
            {
                "pret_asc" => lista.OrderBy(s => s.Servicii.Any()
                    ? s.Servicii.Min(sv => sv.Pret) : 0).ToList(),
                "pret_desc" => lista.OrderByDescending(s => s.Servicii.Any()
                    ? s.Servicii.Max(sv => sv.Pret) : 0).ToList(),
                "rating" => lista.OrderByDescending(s => s.Recenzii.Any()
                    ? s.Recenzii.Average(r => r.Nota) : 0).ToList(),
                _ => lista
            };

            return View(lista);
        }

        [Route("404")]
        public IActionResult Error404() => View();

        [Route("500")]
        public IActionResult Error500() => View();
    }
}