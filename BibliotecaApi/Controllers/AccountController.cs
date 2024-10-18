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
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly BibliotecaContext _context;
        private readonly PasswordValidator _passwordValidator;
        private readonly EmailValidator _emailValidator;
        private IConfiguration _configuration { get; }

        public AccountController(BibliotecaContext context, PasswordValidator passwordValidator,
                                 EmailValidator emailValidator, IConfiguration configuration)
        {
            _context = context;
            _emailValidator = emailValidator;
            _passwordValidator = passwordValidator;
            _configuration = configuration;
        }

        [HttpPost("NewLogin")]
        public async Task<IActionResult> Login(LoginRequest user)
        {
	        var normalUser = AuthenticateNormalUser(user);
	        var adminUser = AuthenticateAdminUser(user);
        
	        if (!(normalUser || adminUser))
		        return Unauthorized();
        
	        var issuer = _configuration["Jwt:Issuer"];
	        var audience = _configuration["Jwt:Audience"];
	        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
	        var claims = new List<Claim>()
	        {
		        new Claim("Id", Guid.NewGuid().ToString()),
		        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
		        new Claim(JwtRegisteredClaimNames.Email, user.Email),
		        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
	        };
        
	        if (adminUser)
	        {
		        claims.Add(new Claim(ClaimTypes.Role, "admin"));
	        }
	        var tokenDescriptor = new SecurityTokenDescriptor
	        {
		        Subject = new ClaimsIdentity(claims),
		        Expires = DateTime.UtcNow.AddMinutes(5), //should be at least 5 minutes - https://github.com/IdentityServer/IdentityServer3/issues/1251
		        Issuer = issuer,
		        Audience = audience,
		        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
	        };
        
	        var tokenHandler = new JwtSecurityTokenHandler();
	        var token = tokenHandler.CreateToken(tokenDescriptor);
	        var stringToken = tokenHandler.WriteToken(token);

	        return Ok(stringToken);
        }

        
        [HttpGet("/secure-page")]
        [Authorize]
        public async Task<IActionResult> SecurePage()
        {
	        return Ok("secure page - for all authenticated users 🔐");
        }
    
        [HttpGet("/admin-page")]
        [Authorize(Policy = "admin_policy")]
        public async Task<IActionResult> AdminPage()
        {
	        return Ok("Admin page - only for admin users 🔐");
        }
    
        private static bool AuthenticateNormalUser(LoginRequest user)
        {
	        //Check the given user credential is valid - Usually this should be checked from database
	        return user.Email == "hello@example.com" && user.Password == "pass123";
        }
        private static bool AuthenticateAdminUser(LoginRequest user)
        {
	        //Check the given user credential is valid - Usually this should be checked from database
	        return user.Email == "admin@example.com" && user.Password == "admin123";
        }
        
        // [HttpPost("Login")]
        // public async Task<IActionResult> Login(LoginRequest request)
        // {
		// }


		// [HttpPost("CrearUsuario")]
        // public async Task<IActionResult> CrearUsuario(CrearUsuarioRequest request)
        // {

		// }

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