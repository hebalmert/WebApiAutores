using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/autores")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AutoresController(ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]  //=> atributos
        [HttpGet("Listado")]  // seria api/autores/Listado
        /*[HttpGet("/Listado")]*/  //=> seria directo Listado
        public async Task<List<AutorDTO>> Get()
        {
            var autores = await _context
                .Autors
                .Include(x=> x.Libros)
                .ToListAsync();

            return _mapper.Map<List<AutorDTO>>(autores);
        }


        //End point o Funciones
        [HttpGet]
        [Route("primero")]
        public async Task<ActionResult<Autor>> PrimerAutor()
        {
            return await _context
                .Autors.FirstOrDefaultAsync();
        }
        //................. End Point o Funciones



        [HttpGet("{id:int}")] //el simbolo de ? es para que el parametro sea opcional
       /* [HttpGet("{id:int}/{param2=persona}")]*/ //igualamos param2 con persona para que no llegue Null
        public async Task<ActionResult<AutorDTO>> Get(int id)
        {
            var autor = await _context.Autors.FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }

            return _mapper.Map<AutorDTO>(autor);
        }



        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            var autores = await _context.Autors.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            if (autores == null)
            {
                return NotFound();
            }

            return _mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]AutoCreacionDTO autoCreacionDTO)
        {
            //Valida si ya existe un autor con el mismo nombre que viene en el modelo
            var existeAutorConElMismoNombre = await _context.Autors.AnyAsync(x => x.Nombre == autoCreacionDTO.Nombre);
            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya Existe un registro con este nombre {autoCreacionDTO.Nombre}");
            }
            var autor = _mapper.Map<Autor>(autoCreacionDTO);

            _context.Autors.Add(autor);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id:int}")]  //api/autores/
        public async Task<ActionResult> Put(Autor autor, int id) 
        {
            if (autor.Id != id)
            {
                //revuelve un erro 400
                return BadRequest("El ID del autor no coincide con el Id de la URL");
            }

            var existe = await _context.Autors.AnyAsync(x=> x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            _context.Autors.Update(autor);
            await _context.SaveChangesAsync(); 
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Autors.AnyAsync(x => x.Id == id);
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
