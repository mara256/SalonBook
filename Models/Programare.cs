using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public enum StatusProgramare
    {
        InAsteptare,
        Acceptata,
        Respinsa,
        Anulata,
        Onorata,
        Neprezentat
    }

    public class Programare
    {
        public int Id { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        [Required]
        public int ServiciuId { get; set; }
        public Serviciu? Serviciu { get; set; }

        [Required]
        public DateTime DataOra { get; set; }

        public StatusProgramare Status { get; set; } = StatusProgramare.InAsteptare;

        public DateTime DataCreare { get; set; } = DateTime.Now;

        public string? Observatii { get; set; }

        public ICollection<Notificare> Notificari { get; set; } = new List<Notificare>();
    }
}