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

using (var scope = app.Services.CreateScope()) {
    var userManager = 
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    string email = "admin@biblioteca.com";
    string password = "Qwerty12345*";

    if(await userManager.FindByEmailAsync(email) == null) {
        var user = new IdentityUser();
        user.UserName = email;
        user.Email = email;

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Administrador");
    }
   
}

app.Run();
