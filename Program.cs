using controlAcademico_web_api.Models;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configurar JSON para ignorar ciclos
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
);

// Configurar el contexto de base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlCons"))
);

// Configuración para SmtpOptions
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

builder.Services.AddMemoryCache();

// Agregar Swagger (para documentación de la API)
builder.Services.AddSwaggerGen();

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:7178", // Cambia esto según tu configuración
        ValidAudience = "https://localhost:7191",      // Cambia esto según tu configuración
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["ApiSettings:jwtSecret"]))
    };
});

// Habilitar políticas de autorización
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar el pipeline de la aplicación
app.UseHttpsRedirection();

// Habilitar HSTS (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // Activa HSTS en entornos de producción
}
// Middleware de autenticación y autorización
app.UseAuthentication();  // Debe ir antes de UseAuthorization()
app.UseAuthorization();
// Añadir encabezados de CSP
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");
    await next();
});

app.MapControllers();

app.Run();
