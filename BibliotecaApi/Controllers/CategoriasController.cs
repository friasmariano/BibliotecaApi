
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BibliotecaApi.Requests;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly List<string> _errors = new List<string>();

        public CategoriasController(BibliotecaContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias
                                   .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoria(int idCategoria)
        {
            var categoria = await _context.Categorias
                                  .Where(r => r.Id == idCategoria)
                                  .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound("La categoría especificada no existe.");
            }

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearCategoriaRequest request)
        {
            ValidateCategoria(request);

            if (!_errors.Any())
            {
                var categoria = new Categoria
                {
                    Nombre = request.Nombre
                };

                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria); 
            }

            return BadRequest(new { Message = _errors });
           
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategoria([FromBody] EditarCategoriaRequest request)
        {
            _errors.Clear();

            var categoria = await _context.Categorias
                                  .Where(r => r.Id == request.CategoriaId)
                                  .FirstOrDefaultAsync();

            if (categoria == null) {
                _errors.Add("La categoría especificada no existe.");
            }

            if (!_errors.Any())
            {
                categoria!.Nombre = request.Nombre;
                _context.Entry(categoria).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Ok(categoria);
            }

            return BadRequest(new { Message = _errors });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] EliminarCategoriaRequest request)
        {
            _errors.Clear();

            var categoria = await _context.Categorias
                                  .Where(r => r.Id == request.CategoriaId)
                                  .FirstOrDefaultAsync();

            if (categoria == null)
            {
                _errors.Add("La categoría especificada no existe.");
            } else
            {
                var librosCategorias = await _context.LibrosCategorias
                                            .Where(r => r.CategoriaId == categoria.Id)
                                            .ToListAsync();

                if (librosCategorias.Count > 0)
                {
                    _errors.Add("No se puede eliminar la categoría; está vinculada a libro(s) registrado(s).");
                }
             }

            if (_errors.Count == 0)
            {
                _context.Categorias.Remove(categoria!);
                await _context.SaveChangesAsync();

                return Ok(categoria);
            }


            return BadRequest(new { Message = _errors });
        }

        private void ValidateCategoria(CrearCategoriaRequest categoria)
        {
            _errors.Clear();

            if (string.IsNullOrEmpty(categoria.Nombre))
            {
                _errors.Add("El campo Nombre es requerido.");
            }
        }
    }
}
