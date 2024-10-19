using System.Security.Claims;
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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaApi.Interfaces;
using BibliotecaApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BibliotecaApi.Responses;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EmailValidator>();
builder.Services.AddScoped<PasswordValidator>();
builder.Services.AddScoped<PasswordHashValidator>();
builder.Services.AddScoped<IUserValidationService, UserValidationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Adding authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero // required if the expire time is <5 minutes
    };
});

//Adding authorization
// builder.Services.AddAuthorization(); //Used if no authorization policy required
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin_policy", policy => policy.RequireRole("admin"));

// Swagger Authorization
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Biblioteca Api", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add default users with BibliotecaContext

using (var scope = app.Services.CreateScope()) {
    var roles = new[] {"Admin", "Bibliotecario"};
    
    // foreach (var role in roles) {
    //     if (!await roleManager.RoleExistsAsync(role))
    //         await roleManager.CreateAsync(new IdentityRole(role));
    // } 
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BibliotecaContext>(); 
    await SeedUsersAsync(context);
}

async Task<IActionResult> CreateUserIfNotExists(string email, string password, string role, 
                          string personaName, BibliotecaContext context) {
    
    var usuario = await context.Usuarios
                        .Where(r => r.Email == email)
                        .FirstOrDefaultAsync();

    if (usuario != null)
    {
        return new BadRequestObjectResult(new { message = "El usuario ya existe." });  
    }

    var person = await context.Personas
                               .Where(r => r.Nombre == personaName)
                               .FirstOrDefaultAsync();
    
    var persona = new Persona() { Nombre = personaName };
    await context.Personas.AddAsync(persona);
    await context.SaveChangesAsync();
        
    var newRole = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == role);

    if (newRole != null) {
        return new BadRequestObjectResult(new { message = "El rol ya existe." });
    }
    
    var rolTemp = new Rol() { Nombre = role };
    context.Roles.Add(rolTemp);
    await context.SaveChangesAsync();
    
    var user = new Usuario()
    {
        Email = email,
        Password = Argon2.Hash(password),
        PersonaId = persona.Id,
        RoldId = rolTemp.Id
    };
    context.Usuarios.Add(user);
    await context.SaveChangesAsync();

    return new OkObjectResult("El usuario ha sido creado con éxito.");
}

async Task SeedUsersAsync(BibliotecaContext context)
{
    var adminUser = await CreateUserIfNotExists("admin@biblioteca.com", "Qwerty12345*", "Admin", "Administrador", context);
    var bibliotecarioUser = await CreateUserIfNotExists("bibliotecario@biblioteca.com", "Qwerty12345*", "Bibliotecario", "Asistente Biblioteca", context);
}

app.Run();
