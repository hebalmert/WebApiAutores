using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Respuestas
{
    public class CredencialesUsuarios
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
