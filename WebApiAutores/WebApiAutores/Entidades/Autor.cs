using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor /*: IValidatableObject*/  //este es para implementar el ValidationResult que esta abajo comentado
    {
        public int Id { get; set; }


        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(maximumLength:120, ErrorMessage ="El campo {0} no debe tener mas de {1} Caracteresa")]
        [PrimeraLetraMayuscula]  //Validacion personalizada en la carpeta Validaciones
        public string Nombre { get; set; }

        //[NotMapped]
        //[Range(18, 120)]
        //public int Edad { get; set; }

        //public int Menor { get; set; }

        //public int Mayor { get; set; }

        public List<Libro> Libros { get; set; }


        //Este tipo de validacion por modelos, se debe hacer pero todas validaciones deben pasar hacia abajo
        //no puede haber validaciones en las propiedades.

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (!string.IsNullOrEmpty(Nombre))
        //    { 
        //        var primeraletra = Nombre[0].ToString();
        //        if (primeraletra != primeraletra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra debe ser Mayuscula",
        //                new string[] { nameof(Nombre) });
        //        }
        //    }


        //    if (Menor > Mayor)
        //    {
        //        yield return new ValidationResult("Este valor no puede ser mas grande que el campo Mayor",
        //            new string[] { nameof(Menor) });
        //    }
        //}
    }
}
