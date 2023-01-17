using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Libro
    {
        public int Id { get; set; }

        [PrimeraLetraMayuscula]  //Validacion personalizada en la carpeta Validaciones
        public string Titulo { get; set; }

        public int? AutorId { get; set;}

        public  Autor Autor { get; set; }
    }
}
