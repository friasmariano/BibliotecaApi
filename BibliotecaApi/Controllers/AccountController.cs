using BibliotecaApi.Models;
using BibliotecaApi.Requests;
using BibliotecaApi.Services;
using BibliotecaApi.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly BibliotecaContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PasswordValidator _passwordValidator;
        private readonly EmailValidator _emailValidator;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, 
                                 RoleManager<IdentityRole> roleManager, BibliotecaContext context, PasswordValidator passwordValidator,
                                 EmailValidator emailValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
        }

        [HttpPost("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioRequest request)
        {
			ValidationResult emailResult = _emailValidator.Validate(request.Email);
			ValidationResult passwordResult = _passwordValidator.Validate(request.Password);

            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

			List<string> errors = new();

			#region Validation(s)
			if (string.IsNullOrEmpty(request.Nombre))
			{
				errors.Clear();

				errors.Add("El nombre no es válido.");

				return BadRequest(errors);
			}

			if (!emailResult.IsValid)
			{
				errors.Clear();

				foreach (var error in emailResult.Errors)
				{
					errors.Add(error.ErrorMessage);
				}

				return BadRequest(errors);
			}

			if (!passwordResult.IsValid)
			{
				errors.Clear();

				foreach (var error in passwordResult.Errors)
				{
					errors.Add(error.ErrorMessage);
				}

				return BadRequest(errors);
			}
			#endregion

			if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                var persona = new Persona
                {
                    Nombre = request.Nombre
                };

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                var personaUser = new PersonaUser
                {
					AspNetUserId = user.Id,
                    PersonaId = persona.Id
				};

                _context.PersonasUser.Add(personaUser);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "El usuario ha sido registrado." });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        [HttpPost("AsignarRol")]
        public async Task<IActionResult> AsignarRol(AsignarRolRequest request)
        {
            List<string> errors = new();
            var rol = await _roleManager.FindByIdAsync(request.RolId);
            var user = await _userManager.FindByEmailAsync(request.Email);

            #region Validation(s)
            if (rol == null) {
                errors.Add("El rol especificado no existe.");
			}

            if (user == null) {
				errors.Add("El usuario especificado no existe.");
			}
			#endregion

			if (errors.Count == 0)
			{
				var result = await _userManager.AddToRoleAsync(user!, rol!.Name!);
				if (result.Succeeded)
				{
					return Ok(new { Message = "El rol ha sido asignado correctamente." });
				}
			}

			return BadRequest(new { Message = errors });
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> Get(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            var persona = await (from pu in _context.PersonasUser
                                 join p in _context.Personas on pu.PersonaId equals p.Id
                                 where pu.AspNetUserId == userId
                                 select new
                                 {
                                     p.Nombre
                                 })
                                 .FirstOrDefaultAsync();


            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { Message = "El Id del usuario no es válido." });
            }
            else
            {
                if (usuario == null)
                {
                    return NotFound(new { Message = "No se ha encontrado ningún usuario con ese id." });
                }

                if (persona == null)
                {
					return NotFound(new { Message = "No se ha encontrado ninguna persona con ese id." });
				}
            }

            return Ok(new { usuario.Id, usuario.UserName, usuario.Email, persona!.Nombre });
        }

        // [HttpPost("AddMainUsersToPersonas")]


		/*
        [HttpPost("Login")]
        [AllowAnonymous]
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
                var token = _JwtTokenService.GenerateToken(user!.Id.ToString(), user!.Rol!.Nombre);
                idUser = user.Id;

                #region TokenParsing
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "exp")?.Value;

                var expDate = new DateTime();

                if (expClaim != null && long.TryParse(expClaim, out long expUnix))
                {
                    expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                }
                #endregion

                var userToken = new UsuarioToken
                {
                    UsuarioId = user.Id,
                    Token_hash = token,
                    CreadoEn = DateTime.Now,
                    ExpiraEn = expDate,
                    Valido = true,
                    Usuario = user,
                };

                _context.UserTokens.Add(userToken);
                await _context.SaveChangesAsync();

                return Ok(new { Token = token });
            }

            return BadRequest(new { Message = errors });
        }

        [HttpPost("CrearUsuario")]
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

            if (rol == null)
            {
                errors.Add("El rol especificado no existe.");
            }
            #endregion

            if (!errors.Any())
            {

                var persona = new Persona
                {
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

            return BadRequest(new { Message = errors });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            var usuario = await _context.Usuarios
                                .Include(r => r.Persona)
                                .Include(r => r.Rol)
                                .Where(e => e.Id == userId)
                                .Select(e => new UsuarioReadResponse
                                {
                                    Id = e.Id,
                                    Nombre = e.Persona!.Nombre,
                                    Email = e.Email,
                                    RolId = e.RolId
                                })
                                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return BadRequest(new { Message = "El usuario especificado no existe." });
            }

            return Ok(usuario);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAll()
        {
            var usuario = await _context.Usuarios
                                .Include(r => r.Persona)
                                .Include(r => r.Rol)
                                .Select(e => new UsuarioReadResponse
                                {
                                    Id = e.Id,
                                    Nombre = e.Persona!.Nombre,
                                    Email = e.Email,
                                    RolId = e.RolId,
                                    Contrasena = ""
                                })
                                .ToListAsync();

            return Ok(usuario);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] EditarUsuarioRequest request)
        {
            #region Declarations
            List<string> errors = new();
            var usuario = await _context.Usuarios
                          .Include(e => e.Persona)
                          .Include(e => e.Rol)
                          .Where(e => e.Id == request.UsuarioId)
                          .FirstOrDefaultAsync();
            ValidationResult emailResult = _emailValidator.Validate(request.Email);
            ValidationResult passwordResult = _passwordValidator.Validate(request.Password);
            #endregion

            #region Validations
            if (usuario == null)
            {
                errors.Add("El usuario no existe.");
            }
            else
            {
                var rol = await _context.Roles
                            .Where(r => r.Id == usuario.RolId)
                            .FirstOrDefaultAsync();

                if (rol == null)
                {
                    errors.Add("El rol especificado no existe.");
                }

            }

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
            #endregion

            if (!errors.Any())
            {
                var persona = await _context.Personas
                                    .Where(e => e.Id == usuario!.PersonaId)
                                    .FirstOrDefaultAsync();

                persona!.Nombre = request.Nombre;
                _context.Entry(persona).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                usuario!.Email = request.Email;
                usuario!.RolId = usuario.RolId;
                usuario!.Password = _passwordHashValidator.GenerateHash(request.Password);

                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(usuario);
            }

            return BadRequest(new { Message = "No ha sido posible modificar el usuario." });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] EliminarUsuarioRequest request)
        {
            List<string> errors = new();
            var usuario = await _context.Usuarios
                                .Include(e => e.Persona)
                                .Include(e => e.Rol)
                                .Where(e => e.Id == request.UsuarioId)
                                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                errors.Add("El usuario especificado no existe.");
            }

            if (!errors.Any())
            {
                var response = usuario;

                var tokens = await _context.UserTokens
                                   .Where(e => e.UsuarioId == usuario!.Id)
                                   .ToListAsync();

                _context.UserTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();

                _context.Usuarios.Remove(usuario!);
                await _context.SaveChangesAsync();

                var persona = await _context.Personas
                                    .Where(e => e.Id == usuario!.Id)
                                    .FirstOrDefaultAsync();

                _context.Personas.Remove(persona!);
                await _context.SaveChangesAsync();

                return Ok(response);
            }

            return BadRequest(new { Message = "No ha sido posible eliminar el usuario." });
        }

    */
    }
 }