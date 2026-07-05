using SalonBook.Models;

namespace SalonBook.Services
{
    public interface IProgramareService
    {
        Task<List<Programare>> GetProgramariClientAsync(string clientId);
        Task<List<Programare>> GetProgramariSalonAsync(int salonId);
        Task<List<DateTime>> GetOreDisponibileAsync(int serviciuId, DateTime data);
        Task<Programare> CreazaProgramareAsync(string clientId, int serviciuId, DateTime dataOra);
        Task<bool> ActualizeazaStatusAsync(int programareId, StatusProgramare status, string detinatorId);
    }
}