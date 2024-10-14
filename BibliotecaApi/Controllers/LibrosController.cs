using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;
using BibliotecaApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Libros")]
    public class LibrosController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly List<string> _errors = new();

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetLibros()
        {
            var libros = await _context.Libros
                                .Include(e => e.Autores)
                                .Include(e => e.Categorias)
                                .ToListAsync();
            return Ok(libros);
        }

        [HttpGet]
        public async Task<IActionResult> GetLibro(long id)
        {
            var libro = await _context.Libros
                        .Where(e => e.Id == id)
                        .FirstOrDefaultAsync();

            if (libro == null)
            {
                return NotFound("El libro no ha sido encontrado");
            }

            return Ok(libro);
        }

        [HttpGet("GetByFechaPublicacion")]
        public async Task<IActionResult> BuscarPorFecha(DateTime fecha)
        {
            var formattedDate = fecha.ToString("yyyy-MM-dd");

            // var libro = await _context.Libros
            //                   .Include(r => r.Autores)
            //                   .Include(r => r.Categorias)
            //                   .Where(e => EF.Functions.Like(e.FechaPublicacion.ToString(), "%" + formattedDate + "%"))
            //                   .ToListAsync();

            var libro = await (from l in _context.Libros
                               join lc in _context.LibrosCategorias on l.Id equals lc.LibroId
                               join c in _context.Categorias on lc.CategoriaId equals c.Id
                               join la in _context.AutoresLibros on l.Id equals la.LibroId
                               join a in _context.Autores on la.AutorId equals a.Id
                               join p in _context.Personas on a.PersonaId equals p.Id
                               where EF.Functions.Like(l.FechaPublicacion.ToString(), "%" + formattedDate + "%")
                               select new {
                                id = l.Id,
                                titulo = l.Titulo,
                                descripcion = l.Descripcion,
                                fecha = l.FechaPublicacion,
                                autor = p.Nombre,
                                categoria = c.Nombre
                               }).ToListAsync();

            if (libro == null)
            {
                return NotFound("No se ha encontrado ningún libro con esa fecha de publicación.");
            }

            return Ok(libro);

        }

        [HttpGet("GetByTitulo")]
        public async Task<IActionResult> BuscarPorTitulo(string titulo)
        {
            var libro = await (from l in _context.Libros
                               join lc in _context.LibrosCategorias on l.Id equals lc.LibroId
                               join c in _context.Categorias on lc.CategoriaId equals c.Id
                               join la in _context.AutoresLibros on l.Id equals la.LibroId
                               join a in _context.Autores on la.AutorId equals a.Id
                               join p in _context.Personas on a.PersonaId equals p.Id
                               where l.Titulo.Contains(titulo)
                               select new {
                                id = l.Id,
                                titulo = l.Titulo,
                                descripcion = l.Descripcion,
                                fecha = l.FechaPublicacion,
                                autor = p.Nombre,
                                categoria = c.Nombre
                               }).ToListAsync();

            if (libro == null)
            {
                return NotFound("No se ha encontrado ningún libro de ese título.");
            }

            return Ok(libro);
        }

        [HttpGet("GetByCategoria")]
        public async Task<IActionResult> BuscarPorCategoria(string categoria)
        {    
            var libro = await (from l in _context.Libros
                               join lc in _context.LibrosCategorias on l.Id equals lc.LibroId
                               join c in _context.Categorias on lc.CategoriaId equals c.Id
                               join la in _context.AutoresLibros on l.Id equals la.LibroId
                               join a in _context.Autores on la.AutorId equals a.Id
                               join p in _context.Personas on a.PersonaId equals p.Id
                               where c.Nombre.Contains(categoria)
                               select new {
                                id = l.Id,
                                titulo = l.Titulo,
                                descripcion = l.Descripcion,
                                fecha = l.FechaPublicacion,
                                autor = p.Nombre,
                                categoria = c.Nombre
                               }).ToListAsync();

            if (libro == null)
            {
                return NotFound("No se ha encontrado ningún libro de ese título.");
            }

            return Ok(libro);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CELibroRequest request)
        {
            await ValidateLibro(request);

            if (_errors.Count == 0)
            {
                var libro = new Libro
                {
                    Titulo = request.Titulo,
                    Descripcion = request.Descripcion,
                    FechaPublicacion = new DateTime(request.FechaPublicacion.Year,
                                                    request.FechaPublicacion.Month,
                                                    request.FechaPublicacion.Day)
                };

                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                var autorLibro = new AutorLibro
                {
                    AutorId = request.AutorId,
                    LibroId = libro.Id
                };

                _context.AutoresLibros.Add(autorLibro);
                await _context.SaveChangesAsync();

                var libroCategoria = new LibroCategoria
                {
                    LibroId = libro.Id,
                    CategoriaId = request.CategoriaId
                };
                _context.LibrosCategorias.Add(libroCategoria);
                await _context.SaveChangesAsync();

                var response = await _context.Libros
                                    .Include(r => r.Autores)
                                    .Include(r => r.Categorias)
                                    .Where(e => e.Id == libro.Id)
                                    .FirstOrDefaultAsync();
                                    

                return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, response);
            }

            return BadRequest(_errors);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLibro([FromBody] CELibroRequest request)
        {
            await ValidateLibro(request);

            if (!_errors.Any())
            {
                var libro = await _context.Libros
                                  .Include(r => r.Autores)
                                  .Include(e => e.Categorias)
                                  .Where(r => r.Id == request.Id)
                                  .FirstOrDefaultAsync();

                libro!.Titulo = request.Titulo;
                libro.Descripcion = request.Descripcion;
                libro.FechaPublicacion = new DateTime(request.FechaPublicacion.Year,
                                                      request.FechaPublicacion.Month,
                                                      request.FechaPublicacion.Day);
                _context.Entry(libro).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                var autorLibro = await _context.AutoresLibros
                                       .Where(r => r.LibroId == libro.Id) 
                                       .FirstOrDefaultAsync();

                autorLibro!.AutorId = request.AutorId;
                await _context.SaveChangesAsync();

                var libroCategoria = await _context.LibrosCategorias
                                           .Where(r => r.LibroId == libro.Id)
                                           .FirstOrDefaultAsync();

                libroCategoria!.CategoriaId = request.CategoriaId;
                await _context.SaveChangesAsync();

                return Ok(libro);
            }

            return BadRequest(_errors);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLibro([FromBody] EliminarLibroRequest request)
        {
            _errors.Clear();

            #region Declarations
            var libro = await _context.Libros
                              .Include(r => r.Autores)
                              .Include(r => r.Categorias)
                              .Where(r => r.Id == request.LibroId)
                              .FirstOrDefaultAsync();

            var autoresLibro = await _context.AutoresLibros
                                   .Where(r => r.LibroId == request.LibroId)
                                   .ToListAsync();

            var libroCategoria = await _context.LibrosCategorias
                                       .Where(e => e.LibroId == request.LibroId)
                                       .FirstOrDefaultAsync();
            #endregion

            #region Validations
            if (libro == null)
            {
                _errors.Add("El libro especificado no existe.");
            }
            #endregion

            if (_errors.Count == 0)
            {
                var response = libro;

                _context.AutoresLibros.RemoveRange(autoresLibro!);
                await _context.SaveChangesAsync();

                _context.LibrosCategorias.Remove(libroCategoria!);
                await _context.SaveChangesAsync();

                _context.Libros.Remove(libro!);
                await _context.SaveChangesAsync();

                return Ok(response);
            }

            return BadRequest(new { Message = _errors });
        }

        private async Task ValidateLibro(CELibroRequest libro)
        {
            _errors.Clear();

            if (string.IsNullOrEmpty(libro.Titulo))
            {
                _errors.Add("El campo título es requerido.");
            }

            if (string.IsNullOrEmpty(libro.Descripcion))
            {
                _errors.Add("El campo descripción es requerida.");
            }

            var categoria = await _context.Categorias
                                  .Where(r => r.Id == libro.CategoriaId)
                                  .FirstOrDefaultAsync();

            if (categoria == null)
            {
                _errors.Add("La categoría no es válida.");
            }

            var autor = await _context.Autores
                              .Where(r => r.Id == libro.AutorId)
                              .FirstOrDefaultAsync();

            if (autor == null) {
                _errors.Add("El autor no es válido.");
            }

            if (libro != null && libro.Id > 0) {

                var book = await _context.Libros
                                  .Where(e => e.Id == libro.Id)
                                  .FirstOrDefaultAsync();

                if (book == null) {
                    _errors.Add("El 'Id' del libro no es válido.");
                }
            }
        }
    }
}
