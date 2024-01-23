using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Campo {0} es Requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El Campo {0} no debe ser mas de {1} caracteres")]
        [PrimeraLetraMayuscula]  //Regla de Validacion Personalizada desde el Folder Validaciones
        public string Nombre { get; set; }

        public List<AutorLibro> AutoresLibros { get; set; }

    }
}
