

using BibliotecaApi.Models;
using BibliotecaApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly JwtTokenService _JwtTokenService;

        public AuthController(BibliotecaContext context, JwtTokenService JwtTokenService)
        {
            _context = context;
            _JwtTokenService = JwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email)
        {
            long? idUser = null; 

            try
            {
                var user = await _context.Usuarios
                            .Include(u => u.Persona)
                            .Include(u => u.Rol)
                            .Where(u => u.Email == email)
                            .FirstOrDefaultAsync();

                if (user != null)
                {
                    var token = _JwtTokenService.GenerateToken(user.Id.ToString(), user.Rol.Nombre);
                    idUser = user.Id;
                    return Ok(new { Token = token });
                }

                return NotFound(new { Message = "Credenciales inválidas." });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Ocurrió un error al procesar la solicitud." + ex });
            }
        }
    }
}
