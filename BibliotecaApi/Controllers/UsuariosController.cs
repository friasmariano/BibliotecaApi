

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers
{
    public class UsuariosController : ControllerBase
    {
        [Authorize(Policy = "Admin")]
        [HttpPost("admin-Only")]
        public async Task<IActionResult> Crear()
        {
            return Ok(StatusCode(200));
        }

    }
}
