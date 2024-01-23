using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatoHATEOAS>>> Get()
        {
            var esadmin = await _authorizationService.AuthorizeAsync(User, "esAdmin");

            var datosHateoas = new List<DatoHATEOAS>();

            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("ObtenerRoot", new { }), descripcion: "self", metodo: "GET"));
            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }), descripcion: "autores", metodo: "GET"));
            if (esadmin.Succeeded)
            {
                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }), descripcion: "Autor-Crear", metodo: "POST"));
                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearLibro", new { }), descripcion: "libro-Crear", metodo: "POST"));
            }
            return datosHateoas;
        }
    }
}
