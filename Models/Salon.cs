using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public enum StatusSalon
    {
        InAsteptare,
        Aprobat,
        Respins
    }

    public class Salon
    {
        public int Id { get; set; }

        [Required]
        public int DetinatorId { get; set; }
        public Detinator? Detinator { get; set; }

        [Required, MaxLength(200)]
        public string Nume { get; set; } = string.Empty;

        public string Descriere { get; set; } = string.Empty;

        public string? PozaUrl { get; set; }

        [Required]
        public string Adresa { get; set; } = string.Empty;
        public TimeSpan OraDeschierii { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan OraInchiderii { get; set; } = new TimeSpan(18, 0, 0);
        public double? Latitudine { get; set; }
        public double? Longitudine { get; set; }

        public bool EsteActiv { get; set; } = false;

        public StatusSalon Status { get; set; } = StatusSalon.InAsteptare;

        public ICollection<Serviciu> Servicii { get; set; } = new List<Serviciu>();

        public ICollection<Recenzie> Recenzii { get; set; } = new List<Recenzie>();
    }
}