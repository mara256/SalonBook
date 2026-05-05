using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class Serviciu
    {
        public int Id { get; set; }

        [Required]
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        [Required, MaxLength(200)]
        public string Nume { get; set; } = string.Empty;

        public string Descriere { get; set; } = string.Empty;

        [Required]
        public string Categorie { get; set; } = string.Empty;

        [Required]
        public int DurataMinte { get; set; }

        [Required]
        public decimal Pret { get; set; }

        public ICollection<Programare> Programari { get; set; } = new List<Programare>();
    }
}
