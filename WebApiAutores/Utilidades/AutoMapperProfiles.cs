using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();
            CreateMap<Autor, AutorDTOConLibros>()
                .ForMember(x=> x.Libros,  opciones => opciones.MapFrom(MapAutorDTOLibros));
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(x => x.AutoresLibros, y => y.MapFrom(MapAutoresLibros));

            CreateMap<LibroPatchDTO, Libro>().ReverseMap();  //Esto permite el mapeo desde LibroPatch hacia Libro y Viceversa

            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>()
                .ForMember(x => x.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();
            if (autor.AutoresLibros == null) { return resultado; }

            foreach (var item in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    Id = item.LibroId,
                    Titulo = item.Libro.Titulo
                });
            }

            return resultado;
        }


        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();
            if (libro.AutoresLibros == null) { return resultado; }

            foreach (var item in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = item.AutorId,
                    Nombre = item.Autor.Nombre
                });
            }

            return resultado;
        }


        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>();
            if (libroCreacionDTO.AutoresIds == null) { return resultado; }

            foreach (var item in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = item });
            }

            return resultado;
        }


    }
}
