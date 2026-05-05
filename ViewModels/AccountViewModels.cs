
using System.ComponentModel.DataAnnotations;

namespace SalonBook.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie")]
        [DataType(DataType.Password)]
        public string Parola { get; set; } = string.Empty;

        public bool TineLogat { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        public string Prenume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefonul este obligatoriu")]
        [Phone(ErrorMessage = "Numar de telefon invalid")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa este obligatorie")]
        public string Adresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie")]
        [MinLength(8, ErrorMessage = "Parola trebuie sa aiba minim 8 caractere")]
        [DataType(DataType.Password)]
        public string Parola { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmarea parolei este obligatorie")]
        [DataType(DataType.Password)]
        [Compare("Parola", ErrorMessage = "Parolele nu coincid")]
        public string ConfirmaParola { get; set; } = string.Empty;

        public string TipCont { get; set; } = "Client";
        public string? DenumireFirma { get; set; }
        public string? AdresaFirma { get; set; }
        public string? TelefonFirma { get; set; }
    }
}