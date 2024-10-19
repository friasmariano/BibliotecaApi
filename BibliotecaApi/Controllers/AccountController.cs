using BibliotecaApi.Models;
using BibliotecaApi.Requests;
using BibliotecaApi.Services;
using BibliotecaApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using BibliotecaApi.Controllers;
using BibliotecaApi.Requests;
using System.Text;
using System.Security.Claims;
using BibliotecaApi.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly PasswordHashValidator _passwordHashValidator;
        private readonly PasswordValidator _passwordValidator;
        private readonly EmailValidator _emailValidator;
        private readonly IUserValidationService _userValidationService;
        private IConfiguration Configuration { get; }

        public AccountController(BibliotecaContext context, PasswordHashValidator passwordHashValidator,
                                 EmailValidator emailValidator, IConfiguration configuration,
                                 IUserValidationService userValidationService,
                                 PasswordValidator passwordValidator)
        {
            _context = context;
            _emailValidator = emailValidator;
            _passwordHashValidator = passwordHashValidator;
            Configuration = configuration;
            _userValidationService = userValidationService;
            _passwordValidator = passwordValidator;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
	        #region Declaration(s)
	        var user = await _context.Usuarios
									.Where(e => e.Email == request.Email)
									.FirstOrDefaultAsync();
	        
	        List<string> errors = new();
	        ValidationResult emailResult = _emailValidator.Validate(request.Email);
	        bool passwordValidator = _passwordHashValidator.ValidatePassword(user!.Password, request.Password);
	        #endregion

	        #region Validation(s)
	        if (!emailResult.IsValid)
	        {
		        foreach (var error in emailResult.Errors)
		        {
			        errors.Add(error.ErrorMessage);
		        }
	        }
	        
	        if (!passwordValidator)
	        {
		        errors.Add("Credenciales inválidas.");
	        }

	        #endregion

	        if (errors.Count == 0)
	        {
		        bool isAdmin = await _userValidationService.IsUserAdminAsync(user!.Id);
	
		        var issuer = Configuration["Jwt:Issuer"];
		        var audience = Configuration["Jwt:Audience"];
		        var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"] ?? string.Empty);
		        
		        var claims = new List<Claim>()
		        {
			        new Claim("Id", user.Id.ToString()),
			        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
			        new Claim(JwtRegisteredClaimNames.Email, user.Email),
			        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		        };
        
		        if (isAdmin)
			        claims.Add(new Claim(ClaimTypes.Role, "admin"));
		        
		        var tokenDescriptor = new SecurityTokenDescriptor
		        {
			        Subject = new ClaimsIdentity(claims),
			        Expires = DateTime.UtcNow.AddHours(5),
			        Issuer = issuer,
			        Audience = audience,
			        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
		        };
        
		        var tokenHandler = new JwtSecurityTokenHandler();
		        var token = tokenHandler.CreateToken(tokenDescriptor);
		        var stringToken = tokenHandler.WriteToken(token);

		        return Ok(stringToken);
	        }
        

	        return BadRequest(new { errors = errors });
        }
        
        [Authorize(Roles = "admin")]
		[HttpPost("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioRequest request)
        {
	        #region Declaration(s)
			List<string> errors = new();
			ValidationResult emailResult = _emailValidator.Validate(request.Email);
			ValidationResult passwordResult = _passwordValidator.Validate(request.Password);

			var rol = await _context.Roles.Where(e => e.Id == request.RolId)
							.FirstOrDefaultAsync();
			#endregion
			
			#region Validation(s)
			if (string.IsNullOrWhiteSpace(request.Nombre))
			{
				errors.Add("El nombre de usuario no es válido.");
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
				foreach (var error in emailResult.Errors)
				{
					errors.Add(error.ErrorMessage);
				}
			}

			if (rol == null)
			{
				errors.Add("El rol especificado no existe.");
			}
			#endregion

			if (errors.Count == 0)
			{
				var person = new Persona()
				{
					Nombre = request.Nombre,
				};
				_context.Personas.Add(person);
				await _context.SaveChangesAsync();

				var user = new Usuario()
				{
					Email = request.Email,
					Password = _passwordHashValidator.GenerateHash(request.Password),
					PersonaId = person.Id,
					RoldId = rol!.Id
				};
				_context.Usuarios.Add(user);
				await _context.SaveChangesAsync();

				return Ok(new { user.Id, user.Email, user.Persona.Nombre });
			}
			
			return BadRequest(new { errors });
        }

        // [HttpPost("AsignarRol")]
        // public async Task<IActionResult> AsignarRol(AsignarRolRequest request)
        // {
        //     List<string> errors = new();
            
        // }

        // [HttpGet("GetUser")]
        // public async Task<IActionResult> Get(string userId)
        // {

		// }

		// [HttpGet("GetAll")]
		// public async Task<IActionResult> GetAll()
		// {
			
		// }

		// [HttpPut("ActualizarUsuario")]
		// public async Task<IActionResult> ActualizarUsuario(string userId, ActualizarUsuarioRequest request)
		// {
			
		// }


		// [HttpDelete("EliminarUsuario")]
		// public async Task<IActionResult> EliminarUsuario(string userId)
		// {
			
		// }

		// [HttpGet("GetRoles")]
		// // Move custom role validation to a service
		// public async Task<IActionResult> GetRoles(string userId)
		// {

		// }

	}
}