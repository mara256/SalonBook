using System.ComponentModel.DataAnnotations;

namespace SalonBook.Models
{
    public class Notificare
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int? ProgramareId { get; set; }
        public Programare? Programare { get; set; }

        [Required]
        public string Mesaj { get; set; } = string.Empty;

        public bool EsteCitita { get; set; } = false;

        public DateTime DataCreare { get; set; } = DateTime.Now;
    }
}
