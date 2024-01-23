using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V2
{
    [Route("api/V2/autores")]
    [Route("api/autores")]  //Asi se deja y se agrega el Attribute que esta en Utilidades para versionar
    [CabeceraEstaPresente("x-version", "2")]  //este es el atributo que filtrara las versiones
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
        [HttpGet("Configuracionesv2")]
        public ActionResult<string> ObtenerConfiguracion()
        {
            //Conseguir el valor dentro de una propieda
            var DesdeAppSetting = _configuration["Apellido"];
            var cadena = _configuration["ConnectionStrings:DefaultConnection"];
            var VariableDeAmbiente = _configuration["ApellidoAmbiente"];
            var VarialeUserSecrets = _configuration["apellidoUserSecrets"];

            return VarialeUserSecrets;
        }


        [HttpGet(Name = "obtenerAutoresv2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            List<Autor> autores = await _context.Autores
                .ToListAsync();
            autores.ForEach(x => x.Nombre = x.Nombre.ToUpper());

            return _mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpGet("{id:int}", Name = "ObtenerAutorv2")] //Atributos 
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




        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev2")] //Atributos 
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await _context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return _mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorv2")]
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
            return CreatedAtRoute("ObtenerAutorv2", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv2")] //api/autores/algo
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

        [HttpDelete("{id:int}", Name = "borrarAutorv2")]  //Atributos  //Todo el metodo se llama tambine EndPoint
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
