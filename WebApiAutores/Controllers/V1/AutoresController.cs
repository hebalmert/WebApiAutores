using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [Route("api/V1/autores")]  // esta es para hacerla por el mismo controlador en la ruta
    [Route("api/autores")]  //Asi se deja y se agrega el Attribute que esta en Utilidades para versionar
    [CabeceraEstaPresente("x-version", "1")]  //este es el atributo que filtrara las versiones
    [ApiConventionType(typeof(DefaultApiConventions))]  //Para que no aparezcan como No Documentada cada Respuesta 200,400 etc. 
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationService _authorizationService;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _authorizationService = authorizationService;
        }

        //Para Extrear informacion del AppSetting usando Iconfiguration
        [HttpGet("Configuracionesv1")]
        public ActionResult<string> ObtenerConfiguracion()
        {
            //Conseguir el valor dentro de una propieda
            var DesdeAppSetting = _configuration["Apellido"];
            var cadena = _configuration["ConnectionStrings:DefaultConnection"];
            var VariableDeAmbiente = _configuration["ApellidoAmbiente"];
            var VarialeUserSecrets = _configuration["apellidoUserSecrets"];

            return VarialeUserSecrets;
        }


        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);

            var autores = await queryable.OrderBy(x=> x.Nombre).Paginar(paginacionDTO).ToListAsync();

            return _mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpGet("{id:int}", Name = "ObtenerAutorv1")] //Atributos 
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
            var autor = await _context.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<AutorDTOConLibros>(autor);


            return dto;
        }




        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev1")] //Atributos 
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await _context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return _mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorv1")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            //Validaciones a ninvel del Controlador
            var existeAutor = await _context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);
            if (existeAutor)
            {
                return BadRequest($"Ya Existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }
            //....

            //Esto es para un sistema Mapper
            var autor = _mapper.Map<Autor>(autorCreacionDTO);

            _context.Add(autor);
            await _context.SaveChangesAsync();

            var autorDTO = _mapper.Map<AutorDTO>(autor);

            // return Ok();  las buenas practicas deben indicar de donde se obtiene los datos y el modelo
            return CreatedAtRoute("ObtenerAutorv1", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv1")] //api/autores/algo
        [ProducesResponseType(404)] //muestra que puede generar estas respuesta el EndPoint
        [ProducesResponseType(200)] //muestra que puede generar estas respuesta el EndPoint
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await _context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            var autor = _mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            _context.Update(autor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Comentario Titulo
        /// </summary>
        /// <param name="id">Comentarios que aparecera en el End Point</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "borrarAutorv1")]  //Atributos  //Todo el metodo se llama tambine EndPoint
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Autor() { Id = id });
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
