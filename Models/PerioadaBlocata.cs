using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class PerioadaBlocata
    {
        public int Id { get; set; }

        [Required]
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        [Required]
        public DateTime DataStart { get; set; }

        [Required]
        public DateTime DataSfarsit { get; set; }

        public string? Motiv { get; set; }

        public DateTime DataCreare { get; set; } = DateTime.Now;
    }
}