using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class Detinator
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(150)]
        public string DenumireFirma { get; set; } = string.Empty;

        [Required]
        public string AdresaFirma { get; set; } = string.Empty;

        [Required, Phone]
        public string Telefon { get; set; } = string.Empty;

        public ICollection<Salon> Saloane { get; set; } = new List<Salon>();
    }
}
