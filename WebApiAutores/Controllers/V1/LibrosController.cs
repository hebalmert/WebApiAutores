using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers.V1
{
    [Route("api/V1/libros")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await _context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return _mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost(Name = "crearLibro")]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se peude crear un Libro sin Autores");
            }

            var autoresIds = await _context.Autores
               .Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
            if (autoresIds.Count != libroCreacionDTO.AutoresIds.Count)
            {
                return BadRequest("No Existe uno de los Autores Enviados");
            }


            var libro = _mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);


            _context.Add(libro);
            await _context.SaveChangesAsync();

            var libroDTO = _mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDb = await _context.Libros.Include(x => x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);
            if (libroDb == null)
            {
                return NotFound();
            }

            var libro = _mapper.Map(libroCreacionDTO, libroDb);
            AsignarOrdenAutores(libro);

            await _context.SaveChangesAsync();
            return NoContent();

        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[0].Orden = i;
                }
            }
        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var libroDb = await _context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            if (libroDb == null)
            {
                return NotFound();
            }

            var libroDTo = _mapper.Map<LibroPatchDTO>(libroDb);

            patchDocument.ApplyTo(libroDTo, ModelState);

            var esValido = TryValidateModel(libroDTo);
            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(libroDTo, libroDb);

            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}", Name = "borrarLibro")]  //Atributos  //Todo el metodo se llama tambine EndPoint
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Libros.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Libro() { Id = id });
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
