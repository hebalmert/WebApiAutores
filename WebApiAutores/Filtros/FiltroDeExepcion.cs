using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filtros
{
    public class FiltroDeExepcion : ExceptionFilterAttribute
    {
        private readonly ILogger<FiltroDeExepcion> logger;

        public FiltroDeExepcion(ILogger<FiltroDeExepcion> logger)
        {
            this.logger = logger;
        }
        //Este filtro Global, que nos va a capturar todos los Errores en Tiempo de ejecucion y lo mandara
        //al Iloger para poderlo ver y corregir.
        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);
            base.OnException(context);
        }
    }
}
