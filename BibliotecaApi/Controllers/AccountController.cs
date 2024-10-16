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
using Sprache;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;

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
        private readonly JwtTokenService _jwtTokenService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager, BibliotecaContext context, PasswordValidator passwordValidator,
                                 EmailValidator emailValidator, JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "El email y la contraseña son obligatorios." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "Usuario no encontrado." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Credenciales incorrectas." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();

            if (roles == null || userRole == null) {
				return BadRequest(new { Message = "El usuario no tiene rol(es) asignado(s)." });
			}
            else
            {
				var token = _jwtTokenService.GenerateToken(user.Id, userRole);

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

				//var userToken = new UsuarioToken
				//{
				//	UsuarioId = user.Id,
				//	Token_hash = token,
				//	CreadoEn = DateTime.Now,
				//	ExpiraEn = expDate,
				//	Valido = true
				//};

				//_context.UserTokens.Add(userToken);
				//await _context.SaveChangesAsync();

				return Ok(new
				{
					Message = "Inicio de sesión exitoso.",
					Token = token,
					User = new
					{
						user.Id,
						user.Email,
						user.UserName
					}
				});
			}
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
            }

            return Ok(new { usuario.Id, usuario.UserName, usuario.Email });
        }

		[HttpGet("GetAllUsers")]
		public async Task<IActionResult> GetAllUsers()
		{
			var usuarios = await _userManager.Users.ToListAsync();

			if (usuarios.Count == 0)
			{
				return NotFound(new { Message = "No se encontraron usuarios." });
			}

			//var userList = from u in usuarios
			//			   join p in personas on u.Id equals p.AspNetUserId into userPersonas
			//			   from persona in userPersonas
			//			   select new
			//			   {
			//				   u.Id,
			//				   u.UserName,
			//				   u.Email,
			//				   Nombre = persona!.Nombre
			//			   };

			return Ok(usuarios);
		}

		[HttpPut("ActualizarUsuario")]
		public async Task<IActionResult> ActualizarUsuario(string userId, ActualizarUsuarioRequest request)
		{
			List<string> errors = new();

			var user = await _userManager.FindByIdAsync(userId);


			#region Validation(s)
			if (user == null)
			{
				return NotFound(new { Message = "Usuario no encontrado." });
			}

			if (string.IsNullOrEmpty(request.Nombre))
			{
				errors.Add("El nombre no es válido.");
				return BadRequest(errors);
			}

			if (string.IsNullOrEmpty(request.Password))
			{
				errors.Clear();
				errors.Add("La contraseña no puede estar vacía.");
				return BadRequest(errors);
			}
			#endregion

			var removePasswordResult = await _userManager.RemovePasswordAsync(user);
			if (!removePasswordResult.Succeeded)
			{
				return BadRequest( new { removePasswordResult.Errors });
			}

			var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);
			if (!addPasswordResult.Succeeded)
			{
				return BadRequest( new { addPasswordResult.Errors });
			}

			var personaUser = await _context.PersonasUser.FirstOrDefaultAsync(pu => pu.AspNetUserId == user.Id);

			if (personaUser != null)
			{
				var persona = await _context.Personas.FindAsync(personaUser.PersonaId);
				if (persona != null)
				{
					persona.Nombre = request.Nombre;
					_context.Personas.Update(persona);
					await _context.SaveChangesAsync();
				}
			}
			else
			{
				return NotFound(new { Message = "Persona asociada no encontrada." });
			}

			return Ok(new { Message = "El nombre y la contraseña han sido actualizados correctamente." });
		}


		[HttpDelete("EliminarUsuario")]
		public async Task<IActionResult> EliminarUsuario(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return NotFound(new { Message = "Usuario no encontrado." });
			}

			var personaUser = await _context.PersonasUser.FirstOrDefaultAsync(pu => pu.AspNetUserId == user.Id);

			if (personaUser != null)
			{
				var persona = await _context.Personas.FindAsync(personaUser.PersonaId);

				if (persona != null)
				{
					_context.Personas.Remove(persona);
				}

				_context.PersonasUser.Remove(personaUser);
				await _context.SaveChangesAsync();
			}

			var result = await _userManager.DeleteAsync(user);

			if (result.Succeeded)
			{
				return Ok(new { Message = "Usuario y sus datos asociados han sido eliminados correctamente." });
			}

			return BadRequest(new { Errors = result.Errors });
		}

		// ROLES CRUD
	}
}