using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet]
        public async Task<ActionResult<List<LibroDTO>>> GetLibro()
        {
            var libros =await _context.Libros.ToListAsync();
            return _mapper.Map<List<LibroDTO>>(libros);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {

            var libro = await _context.Libros
                .FirstOrDefaultAsync(x => x.Id == id);

            return _mapper.Map<LibroDTO>(libro);
            
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            //var existeAutor = await _context.Autors.AnyAsync(x => x.Id == libro.AutorId);
            //if (!existeAutor) 
            //{
            //    return BadRequest($"No Existe el autor id: {libro.AutorId}");
            //}
            var libro = _mapper.Map<Libro>(libroCreacionDTO);
            _context.Add(libro);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
