using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;

namespace SalonBook.Services
{
    public class ProgramareService : IProgramareService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificareService _notificareService;

        public ProgramareService(ApplicationDbContext context, INotificareService notificareService)
        {
            _context = context;
            _notificareService = notificareService;
        }

        public async Task<List<Programare>> GetProgramariClientAsync(string clientId)
        {
            return await _context.Programari
                .Include(p => p.Serviciu)
                    .ThenInclude(s => s!.Salon)
                .Where(p => p.ClientId == clientId)
                .OrderByDescending(p => p.DataOra)
                .ToListAsync();
        }

        public async Task<List<Programare>> GetProgramariSalonAsync(int salonId)
        {
            return await _context.Programari
                .Include(p => p.Client)
                .Include(p => p.Serviciu)
                .Where(p => p.Serviciu!.SalonId == salonId)
                .OrderBy(p => p.DataOra)
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetOreDisponibileAsync(int serviciuId, DateTime data)
        {
            var serviciu = await _context.Servicii.FindAsync(serviciuId);
            if (serviciu == null) return new List<DateTime>();

            var oreDisponibile = new List<DateTime>();
            var start = data.Date.AddHours(9);
            var sfarsit = data.Date.AddHours(18);

            var programariExistente = await _context.Programari
                .Where(p => p.ServiciuId == serviciuId
                    && p.DataOra.Date == data.Date
                    && p.Status != StatusProgramare.Respinsa
                    && p.Status != StatusProgramare.Anulata)
                .Select(p => p.DataOra)
                .ToListAsync();

            for (var ora = start; ora.AddMinutes(serviciu.DurataMinte) <= sfarsit; ora = ora.AddMinutes(30))
            {
                if (ora < DateTime.Now) continue;

                bool ocupat = programariExistente.Any(p =>
                    ora < p.AddMinutes(serviciu.DurataMinte) && ora.AddMinutes(serviciu.DurataMinte) > p);

                if (!ocupat)
                    oreDisponibile.Add(ora);
            }

            return oreDisponibile;
        }

        public async Task<Programare> CreazaProgramareAsync(string clientId, int serviciuId, DateTime dataOra)
        {
            if (dataOra < DateTime.Now)
                throw new InvalidOperationException("Nu poți face o programare în trecut.");

            var programare = new Programare
            {
                ClientId = clientId,
                ServiciuId = serviciuId,
                DataOra = dataOra,
                Status = StatusProgramare.InAsteptare,
                DataCreare = DateTime.Now
            };

            _context.Programari.Add(programare);
            await _context.SaveChangesAsync();

            var serviciu = await _context.Servicii
                .Include(s => s.Salon)
                    .ThenInclude(s => s!.Detinator)
                .FirstOrDefaultAsync(s => s.Id == serviciuId);

            if (serviciu?.Salon?.Detinator != null)
            {
                await _notificareService.TrimiteNotificareAsync(
                    serviciu.Salon.Detinator.UserId,
                    programare.Id,
                    $"Programare noua pentru {serviciu.Nume} pe {dataOra:dd MMM yyyy HH:mm}");
            }

            return programare;
        }

        public async Task<bool> ActualizeazaStatusAsync(int programareId, StatusProgramare status, string detinatorId)
        {
            var programare = await _context.Programari
                .Include(p => p.Serviciu)
                    .ThenInclude(s => s!.Salon)
                        .ThenInclude(s => s!.Detinator)
                .FirstOrDefaultAsync(p => p.Id == programareId);

            if (programare == null) return false;

            if (programare.Serviciu?.Salon?.Detinator?.UserId != detinatorId)
                return false;

            programare.Status = status;
            await _context.SaveChangesAsync();

            string mesaj = status == StatusProgramare.Acceptata
                ? $"Programarea ta din {programare.DataOra:dd MMM yyyy HH:mm} a fost acceptata!"
                : $"Programarea ta din {programare.DataOra:dd MMM yyyy HH:mm} a fost respinsa.";

            await _notificareService.TrimiteNotificareAsync(programare.ClientId, programareId, mesaj);

            return true;
        }
    }
}