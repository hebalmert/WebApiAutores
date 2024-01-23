using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroPatchDTO
    {
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250, ErrorMessage = "El Campo {0} no debe ser mas de {1} caracteres")]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
