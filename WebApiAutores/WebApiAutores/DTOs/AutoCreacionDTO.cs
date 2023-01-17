using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class AutoCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener mas de {1} Caracteresa")]
        [PrimeraLetraMayuscula]  //Validacion personalizada en la carpeta Validaciones
        public string Nombre { get; set; }
    }
}
