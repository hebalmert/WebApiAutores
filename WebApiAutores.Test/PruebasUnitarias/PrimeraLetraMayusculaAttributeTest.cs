using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Test.PruebasUnitarias
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributeTest
    {
        [TestMethod]
        public void PrimeraLetraMinuscula_DevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var valor = "hebert";
            var valContext = new ValidationContext(new { Nombre = valor });

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);


            //Verificacion
            Assert.AreEqual("La Primera Letra debe ser Mayuscula", resultado!.ErrorMessage);

        }


        [TestMethod]
        public void ValorNulo_NoDevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            string? valor = null;
            var valContext = new ValidationContext(new { Nombre = valor });

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);


            //Verificacion
            Assert.IsNull(resultado);

        }

        [TestMethod]
        public void ValorConPrimeraLetraMayuscula_NoDevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            string? valor = "Felipe";
            var valContext = new ValidationContext(new { Nombre = valor });

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);


            //Verificacion
            Assert.IsNull(resultado);

        }
    }
}