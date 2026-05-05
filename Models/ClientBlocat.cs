using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class ClientBlocat
    {
        public int Id { get; set; }

        [Required]
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        public DateTime DataBlocare { get; set; } = DateTime.Now;

        public string? Motiv { get; set; }
    }
}