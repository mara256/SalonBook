using Microsoft.AspNetCore.Identity;

namespace SalonBook.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string Adresa { get; set; } = string.Empty;
        public string Rol { get; set; } = "Client";  

        public ICollection<Programare> Programari { get; set; } = new List<Programare>();
    }
}
