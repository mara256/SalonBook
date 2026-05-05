using Microsoft.EntityFrameworkCore;
using SalonBook.Data;
using SalonBook.Models;

namespace SalonBook.Services
{
    public interface INotificareService
    {
        Task TrimiteNotificareAsync(string userId, int programareId, string mesaj);
        Task<List<Notificare>> GetNotificariAsync(string userId);
        Task MarcheazaCititaAsync(int notificareId, string userId);
        Task<int> GetNrNecititeAsync(string userId);
    }

    public class NotificareService : INotificareService
    {
        private readonly ApplicationDbContext _context;

        public NotificareService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task TrimiteNotificareAsync(string userId, int programareId, string mesaj)
        {
            var notificare = new Notificare
            {
                UserId = userId,
                ProgramareId = programareId,
                Mesaj = mesaj,
                EsteCitita = false,
                DataCreare = DateTime.Now
            };
            _context.Notificari.Add(notificare);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notificare>> GetNotificariAsync(string userId)
        {
            return await _context.Notificari
                .Include(n => n.Programare)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DataCreare)
                .ToListAsync();
        }

        public async Task MarcheazaCititaAsync(int notificareId, string userId)
        {
            var notificare = await _context.Notificari
                .FirstOrDefaultAsync(n => n.Id == notificareId && n.UserId == userId);
            if (notificare != null)
            {
                notificare.EsteCitita = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNrNecititeAsync(string userId)
        {
            return await _context.Notificari
                .CountAsync(n => n.UserId == userId && !n.EsteCitita);
        }
    }
}
