using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios
{
    public class GenerardorEnlaces
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly HttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;

        public GenerardorEnlaces(IAuthorizationService authorizationService, HttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _actionContextAccessor = actionContextAccessor;
        }

        private IUrlHelper contruirURLHelper()
        {
            var factoria = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(_actionContextAccessor.ActionContext);
        }
        private async Task<bool> EsAdmin()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var resultado = await _authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;
        }

        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = contruirURLHelper();

            autorDTO.Enlaces.Add(new DatoHATEOAS(
                enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                descripcion: "Self",
                metodo: "GET"));
            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(
                    enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                    descripcion: "autor-actualizar",
                    metodo: "PUT"));
                autorDTO.Enlaces.Add(new DatoHATEOAS(
                    enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                    descripcion: "Self",
                    metodo: "DELETE"));
            }
        }
    }
}
