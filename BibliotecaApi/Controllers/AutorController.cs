

using Azure.Core;
using BibliotecaApi.Models;
using BibliotecaApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Autores")]
    public class AutorController : ControllerBase
    {
        
        private readonly BibliotecaContext _context;

        public AutorController(BibliotecaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearAutorRequest request)
        {
            #region Declaration(s)
            List<string> errors = new();
            #endregion

            #region Validation(s)
            if (string.IsNullOrEmpty(request.Nombre))
            {
                errors.Add("El nombre no es válido.");
            }
            #endregion

            if (!errors.Any())
            {
                var person = new Persona
                {
                    Nombre = request.Nombre
                };

                await _context.Personas.AddAsync(person);
                await _context.SaveChangesAsync();

                var autor = new Autor
                {
                    PersonaId = person.Id
                };

                await _context.Autores.AddAsync(autor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Crear), new { autorId = autor.Id }, autor);
            }

            return BadRequest(new { Message = errors });
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll() {
            var autores = await _context.Autores
                        .Include(e => e.Persona)
                        .ToListAsync();

            return Ok(autores);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int autorId) {
            var autor = await _context.Autores
                        .Include(e => e.Persona)
                        .Where(e => e.Id == autorId)
                        .FirstOrDefaultAsync();

            if (autor == null) {
                return BadRequest(new { Message = "El autor especificado no existe." });
            }

            return Ok(autor);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] EditarAutorRequest request)
        {
            List<string> errors = new();
            var autor = await _context.Autores
                              .Where(e => e.Id == request.AutorId)
                              .FirstOrDefaultAsync();

            if (autor == null) {
                errors.Add("El autor no existe.");
            }

            if (!errors.Any()) { 
                var persona = await _context.Personas
                               .Where(e => e.Id == autor!.PersonaId)
                               .FirstOrDefaultAsync();

                persona!.Nombre = request.Nombre;
                _context.Entry(persona).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok (autor);
            }

            return BadRequest(new { Message = errors });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromBody] CEAutorRequest request)
        {
            List<string> errors = new();
            var autor = await _context.Autores
                              .Where(e => e.Id == request.AutorId)
                              .FirstOrDefaultAsync();

            #region Validation(s)
            if (autor == null)
            {
                errors.Add("El autor no existe.");
            }
            else
            {
                var autoresLibros = await _context.Autores
                                          .Where(r => r.Id == autor.Id)
                                          .ToListAsync();

                if (autoresLibros.Count > 0)
                {
                    errors.Add("No se puede eliminar el autor; está vinculado a libro(s) registrado(s).");
                }

            }
            #endregion

            if (errors.Count == 0)
            {
                var response = new Autor
                {
                    Id = autor!.Id,
                    PersonaId = autor.PersonaId
                };

                long idAutor = autor!.Id;

                _context.Autores.Remove(autor!);
                await _context.SaveChangesAsync();

                var persona = await _context.Personas
                               .Where(e => e.Id == autor!.PersonaId)
                               .FirstOrDefaultAsync();

                _context.Personas.Remove(persona!);
                await _context.SaveChangesAsync();

                return Ok(response);
            }

            return BadRequest(new { Message = "No se ha podido eliminar el autor." });
        }
    }
}
