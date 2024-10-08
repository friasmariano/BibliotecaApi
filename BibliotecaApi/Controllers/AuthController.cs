

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
            try
            {
                var user = await _context.Usuarios
                            .Where(u => u.Email == email)
                            .FirstOrDefaultAsync();

                if (user != null && user.Rol != null && !string.IsNullOrEmpty(user.Rol.Nombre))
                {
                    var token = _JwtTokenService.GenerateToken(user.Id.ToString(), user.Rol.Nombre);
                    return Ok(new { Token = token });
                }

                return NotFound(new { Message = "Credenciales inválidas." });
            }
            catch(Exception e)
            {
                return StatusCode(500, new { Message = "Ocurrió un error al procesar la solicitud."});
            }
        }
    }
}
