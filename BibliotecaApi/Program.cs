using BibliotecaApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using System.Text;
using BibliotecaApi.Services;
using FluentValidation;
using BibliotecaApi.Models;
using BibliotecaApi.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using System.Data.Entity;
using BibliotecaApi.Responses;
using Sprache;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Env.Load();

var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") + "";

builder.Services.AddControllers();

builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<BibliotecaContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});


// Adding Identity services with roles
// builder.Services.AddDefaultIdentity<IdentityUser>(options =>
// {
//     options.SignIn.RequireConfirmedAccount = true;
// })
// .AddRoles<IdentityRole>()  // Enable role management
// .AddEntityFrameworkStores<BibliotecaContext>();  // Use EF Core for storing identity info

////////////////////////
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddScoped<EmailValidator>();
builder.Services.AddScoped<PasswordValidator>();
builder.Services.AddScoped<PasswordHashValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    var roleManager = 
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] {"Administrador", "Bibliotecario"};

    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;

	var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

	var context = services.GetRequiredService<BibliotecaContext>(); 

	await SeedUsersAsync(userManager, roleManager, context);
}

async Task<UserCreationResult> CreateUserIfNotExists(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string email, string password, string role, string personaName,
											   BibliotecaContext context)
{
	if (await userManager.FindByEmailAsync(email) != null)
	{
		return new UserCreationResult
		{
			Success = false,
			Message = "Ya existe un usuario con este email."
		};
	}

	var user = new IdentityUser
	{
		UserName = email,
		Email = email
	};

	var result = await userManager.CreateAsync(user, password);

	if (result.Succeeded)
	{
		if (await roleManager.FindByNameAsync(role) == null)
		{
			return new UserCreationResult
			{
				Success = false,
				Message = "El rol especificado no existe."
			};
		}

		await userManager.AddToRoleAsync(user, role);

		var persona = new Persona { Nombre = personaName };
		await context.Personas.AddAsync(persona);
		await context.SaveChangesAsync();

		await context.PersonasUser.AddAsync(new PersonaUser
		{
			AspNetUserId = user.Id,
			PersonaId = persona.Id
		});
		await context.SaveChangesAsync();

		return new UserCreationResult
		{
			Success = true,
			Message = "El usuario ha sido creado.",
			User = user
		};
	}

	return new UserCreationResult
	{
		Success = false,
		Message = "No fue posible crear el usuario: " + string.Join(", ", result.Errors.Select(e => e.Description))
	};
}

async Task SeedUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, BibliotecaContext context)
{
	var adminUser = await CreateUserIfNotExists(userManager, roleManager, "admin@biblioteca.com", "Qwerty12345*", "Administrador", "Administrador", context);
	var bibliotecarioUser = await CreateUserIfNotExists(userManager, roleManager, "bibliotecario@biblioteca.com", "Qwerty12345*", "Bibliotecario", "Bibliotecario", context);
}

app.Run();
