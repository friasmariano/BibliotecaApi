

using BibliotecaApi.Models;
using BibliotecaApi.Requests;
using BibliotecaApi.Services;
using BibliotecaApi.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly JwtTokenService _JwtTokenService;
        private readonly EmailValidator _emailValidator;
        private readonly PasswordValidator _passwordValidator;
        private readonly PasswordHashValidator _passwordHashValidator;

        public AccountController(BibliotecaContext context, JwtTokenService JwtTokenService, EmailValidator emailValidator,  PasswordHashValidator passwordHashValidator, PasswordValidator passwordValidator)
        {
            _context = context;
            _JwtTokenService = JwtTokenService;
            _emailValidator = emailValidator;
            _passwordHashValidator = passwordHashValidator;
            _passwordValidator = passwordValidator;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            #region Declaration(s)
            long? idUser = null;
            List<string> errors = new();
            ValidationResult emailResult = _emailValidator.Validate(request.Email);
            var user = await _context.Usuarios
                            .Include(u => u.Persona)
                            .Include(u => u.Rol)
                            .Where(u => u.Email == request.Email)
                            .FirstOrDefaultAsync();
            #endregion

            #region Validations
            if (!emailResult.IsValid)
            {
                foreach (var error in emailResult.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            else
            {
                if (user == null)
                {
                    errors.Add("Credenciales inválidas.");
                } 
                else
                {
                    bool isValid = _passwordHashValidator.ValidatePassword(user.Password, request.Password);

                    if (!isValid)
                    {
                        return Unauthorized("El usuario o la clave es inválido.");
                    }
                }
            }
            #endregion

            if (!errors.Any())
            {
                var token = _JwtTokenService.GenerateToken(user.Id.ToString(), user.Rol.Nombre);
                idUser = user.Id;

                return Ok(new { Token = token });
            }

            return BadRequest(new { Message = errors });
        }

        [HttpPost("CrearUsuario")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequest request)
        {
            #region Declaration(s)
            List<string> errors = new();
            ValidationResult emailResult = _emailValidator.Validate(request.Email);
            ValidationResult passwordResult = _passwordValidator.Validate(request.Password);
            var rol = await _context.Roles
                        .Where(r => r.Id == request.RolId)
                        .FirstOrDefaultAsync();
            #endregion

            #region Validation(s)
            if (string.IsNullOrEmpty(request.Nombre))
            {
                errors.Add("El nombre no es válido.");
            }

            if (!emailResult.IsValid)
            {
                foreach (var error in emailResult.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            if (!passwordResult.IsValid)
            {
                foreach (var error in passwordResult.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            if (rol == null) {
                errors.Add("El rol especificado no existe.");
            }
            #endregion

            if (!errors.Any()) {

                var persona = new Persona { 
                    Nombre = request.Nombre 
                };

                await _context.Personas.AddAsync(persona);
                await _context.SaveChangesAsync();

                var usuario = new Usuario
                {
                    Email = request.Email,
                    Password = _passwordHashValidator.GenerateHash(request.Password),
                    PersonaId = persona.Id,
                    RolId = request.RolId
                };

                await _context.Usuarios.AddAsync(usuario);
                await _context.SaveChangesAsync();

                Response.StatusCode = StatusCodes.Status201Created;
                return CreatedAtAction(nameof(CrearUsuario), new { id = usuario.Id }, usuario);
            }

            return BadRequest(new { Message = errors } );
        }
    }
}
