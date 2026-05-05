using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class Recenzie
    {
        public int Id { get; set; }

        [Required]
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        [Required]
        [Range(1, 5)]
        public int Nota { get; set; }

        [MaxLength(500)]
        public string? Comentariu { get; set; }

        public DateTime DataCreare { get; set; } = DateTime.Now;
    }
}