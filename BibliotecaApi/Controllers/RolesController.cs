
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;
//using BibliotecaApi.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.EntityFrameworkCore;
//using BibliotecaApi.Requests;

//namespace BibliotecaApi.Controllers
//{
//    [ApiController]
//    [Route("api/Roles")]
//    public class RolesController : ControllerBase
//    {
//        private readonly BibliotecaContext _context;
//        private readonly List<string> _errors = new List<string>();

//        public RolesController(BibliotecaContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("GetAll")]
//        public async Task<IActionResult> GetAll()
//        {
//            var roles = await _context.Roles
//                              .ToListAsync();

//            return Ok(roles);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Get(int rolId)
//        {
//            var rol = await _context.Roles
//                            .Where(r => r.Id == rolId)
//                            .FirstOrDefaultAsync();

//            if (rol == null)
//            {
//                return NotFound();
//            }

//            return Ok(rol);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] CrearRolRequest request)
//        {
//            _errors.Clear();

//            if (string.IsNullOrEmpty(request.Nombre)) {
//                _errors.Add("El nombre especificado no es válido.");
//            }

//            if (_errors.Count == 0)
//            {
//                var rol = new Rol
//                {
//                    Nombre = request.Nombre
//                };

//                _context.Roles.Add(rol);
//                await _context.SaveChangesAsync();

//                return CreatedAtAction(nameof(Create), new { id = rol.Id }, rol);  
//            }

//            return BadRequest(new { Message = _errors });
//        }

//        [HttpPut]
//        public async Task<IActionResult> Update([FromBody] EditarRolRequest request)
//        {
//            _errors.Clear();

//            var rol = await _context.Roles
//                            .Where(r => r.Id == request.RoldId)
//                            .FirstOrDefaultAsync();

//            #region Validations
//            if (rol == null)
//            {
//                _errors.Add("El rol especificado no existe.");
//            }

//            if (string.IsNullOrEmpty(request.Nombre)) {
//                _errors.Add("El nombre del rol no es válido.");
//            }
//            #endregion 

//            if (_errors.Count == 0)
//            {

//                rol!.Nombre = request.Nombre;

//                await _context.SaveChangesAsync();

//                return Ok(rol);
//            }

//            return BadRequest(new { Messages = _errors });
//        }

//        [HttpDelete]
//        public async Task<IActionResult> DeleteRol([FromBody] EliminarRolRequest request)
//        {
//            _errors.Clear();
//            var rol = await _context.Roles
//                            .Where(r => r.Id == request.RolId) 
//                            .FirstOrDefaultAsync();

//            #region Validation(s)
//            if (rol == null)
//            {
//                _errors.Add("El rol especificado no existe.");
//            } else
//            {
//                var usuario = await _context.Usuarios
//                                    .Where(r => r.RolId == rol.Id)
//                                    .ToListAsync();

//                if (usuario.Count > 0) {
//                    _errors.Add("No se puede eliminar el rol, porque está vinculado a usuario(s) registrado(s).");
//                }
//            }
//            #endregion

//            if (_errors.Count == 0)
//            {
//                var response = rol;

//                _context.Roles.Remove(rol!);
//                await _context.SaveChangesAsync();

//                return Ok(response);
//            }

//            return BadRequest(new { Message  = _errors });
//        }

//        private void ValidateRol(CrearRolRequest rol)
//        {
//            _errors.Clear();

//            if (string.IsNullOrEmpty(rol.Nombre))
//            {
//                _errors.Add("El campo Nombre es requerido.");
//            }
//        }
//    }
//}
