

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Libros")]
    public class LibrosController: ControllerBase
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        //[HttpPost("Crear")]
        //public Task<IActionResult> Crear()
        //{

        //}
    }
}
