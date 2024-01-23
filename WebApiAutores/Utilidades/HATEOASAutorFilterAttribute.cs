using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Utilidades
{
    public class HATEOASAutorFilterAttribute : HATEOASFiltroAttribute
    {
        private readonly GenerardorEnlaces _generardorEnlaces;

        public HATEOASAutorFilterAttribute(GenerardorEnlaces generardorEnlaces)
        {
            _generardorEnlaces = generardorEnlaces;
        }
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var debeIncluir = DebeIncluirHATEOAS(context);

            if (!debeIncluir)
            {
                await next();
                return;
            }

            var resultado = context.Result as ObjectResult;

            var autorDTO = resultado.Value as AutorDTO;
            if (autorDTO == null)
            {
                var autoresDTO = resultado.Value as List<AutorDTO> ?? throw new ArgumentException("Se Esperaba una Instancia de AutoDTO" +
                    " o List<AutorDTO>");

                autoresDTO.ForEach(async autor => await _generardorEnlaces.GenerarEnlaces(autor));
                resultado.Value = autoresDTO;
            }
            else
            { 
                await _generardorEnlaces.GenerarEnlaces(autorDTO);
            }

            
            await next();
        }
    }
}
