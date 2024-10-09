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
    [Authorize]
    [Route("api/Libros")]
    public class LibrosController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly List<string> _errors = new();

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CELibroRequest request)
        {
            ValidateLibro(request);

            if (!_errors.Any())
            {
                var libro = new Libro
                {
                    Titulo = request.Titulo,
                    Descripcion = request.Descripcion,
                    FechaPublicacion = request.FechaPublicacion
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
            ValidateLibro(request);

            if (!_errors.Any())
            {
                var libro = await _context.Libros
                                    .Where(r => r.Id == request.Id)
                                    .FirstOrDefaultAsync();

                libro!.Titulo = request.Titulo;
                libro.Descripcion = request.Descripcion;
                libro.FechaPublicacion = request.FechaPublicacion;

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

                return NoContent();
            }

            return BadRequest(_errors);
        }

        // DELETE: api/Libros/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteLibro(int id)
        {
            var libro = _context.Libros.Find(id);
            if (libro == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(libro);
            _context.SaveChanges();

            return NoContent();
        }

        private async void ValidateLibro(CELibroRequest libro)
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

            if (libro != null) {

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
