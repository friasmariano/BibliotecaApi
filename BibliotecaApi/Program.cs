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

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Env.Load();

var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") + "";

builder.Services.AddControllers();

builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthorization(options =>
{
	//options.AddPolicy("Admin", policy =>
	//    policy.RequireClaim("role", "Administrador"));
	options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser()); // Allow authenticated users
});

//builder.Services.AddAuthorization(options =>
//{
//	options.AddPolicy("Admin", policy =>
//		policy.RequireAssertion(context =>
//			context.User.HasClaim(c => c.Type == "role" && c.Value == "Administrador")));
//});

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


app.Run();
