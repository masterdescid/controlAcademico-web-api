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

// Configuraci�n para SmtpOptions
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

builder.Services.AddMemoryCache();

// Agregar Swagger (para documentaci�n de la API)
builder.Services.AddSwaggerGen();

// Configurar autenticaci�n JWT
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
        ValidIssuer = "https://localhost:7178", // Cambia esto seg�n tu configuraci�n
        ValidAudience = "https://localhost:7191",      // Cambia esto seg�n tu configuraci�n
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["ApiSettings:jwtSecret"]))
    };
});

// Habilitar pol�ticas de autorizaci�n
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar el pipeline de la aplicaci�n
app.UseHttpsRedirection();

// Habilitar HSTS (solo en producci�n)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // Activa HSTS en entornos de producci�n
}
// Middleware de autenticaci�n y autorizaci�n
app.UseAuthentication();  // Debe ir antes de UseAuthorization()
app.UseAuthorization();
// A�adir encabezados de CSP
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");
    await next();
});

app.MapControllers();

app.Run();
