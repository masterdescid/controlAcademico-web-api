using controlAcademico_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;



namespace controlAcademico_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly SmtpOptions _smtpOptions;

        public UsuarioController(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache, IOptions<SmtpOptions> smtpOptions)
        {
            _context = context;
            _configuration = configuration;
            _cache = cache;
            _smtpOptions = smtpOptions.Value;
        }
        [HttpPost("enviar-codigo-verificacion")]
        public async Task<IActionResult> EnviarCodigoVerificacion([FromBody] string email)
        {
            try
            {
                string codigoVerificacion = GenerarCodigo();

                // Almacenar el código en caché
                string cacheKey = $"codigo_verificacion_{email}";
                _cache.Set(cacheKey, codigoVerificacion, TimeSpan.FromMinutes(2));

                // Enviar el correo de verificación
                await EnviarCorreoVerificacion(email, codigoVerificacion);

                return Ok("Código de verificación enviado. Por favor, revisa tu correo.");
            }
            catch (Exception ex)
            {
                // Puedes registrar el error aquí si es necesario
                return BadRequest("Error al enviar el código de verificación: " + ex.Message);
            }
        }

        private string GenerarCodigo()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task EnviarCorreoVerificacion(string correoDestino, string codigoVerificacion)
        {
            var mensaje = new MailMessage();
            mensaje.To.Add(correoDestino);
            mensaje.Subject = "Código de Verificación";
            mensaje.Body = $"Tu código de verificación es: {codigoVerificacion}";
            mensaje.From = new MailAddress(_smtpOptions.UserName);

            using (var smtp = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port))
            {
                smtp.Credentials = new NetworkCredential(_smtpOptions.UserName, _smtpOptions.Password);
                smtp.EnableSsl = _smtpOptions.EnableSsl;

                await smtp.SendMailAsync(mensaje);

            }
        }

        [HttpPost("verificar-codigo")]
        public IActionResult VerificarCodigo([FromBody] VerificarCodigoRequest request)
        {
            string cacheKey = $"codigo_verificacion_{request.Email}";

            // Intenta obtener el código almacenado en caché
            if (_cache.TryGetValue(cacheKey, out string codigoAlmacenado))
            {
                // Compara el código almacenado con el código proporcionado
                if (codigoAlmacenado == request.Codigo)
                {
                    return Ok(true);  // Código correcto
                }
                else
                {
                    return Ok(false); // Código incorrecto
                }
            }
            else
            {
                return BadRequest("El código de verificación ha expirado o no existe.");
            }
        }

        // GET: api/<AgenciaController>
        [HttpGet]
        [AuthorizeRole("Administrador", "Administrador")]
        public async Task<ActionResult<List<usuario>>> Get()
        {
            try
            {
                List<usuario>? objetoModel = new List<usuario>();
                objetoModel = await _context.usuario.Where(x => x.estatus == 1 || x.estatus == 2).ToListAsync();

                if (objetoModel != null)
                {
                    foreach (var usuario in objetoModel)
                    {
                        usuario.correo = EncryptionHelper.Decrypt(usuario.correo);
                    }
                    return Ok(objetoModel);
                }
                else
                {
                    return BadRequest("Se obtuvo un NULL como respuesta en la búsqueda.");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(String.Concat("Error: ", ex.Message));
            }
        }

        // GET api/<AgenciaController>/5
        [HttpGet("{codigoUsuario:int}")]
        [AuthorizeRole("Administrador","Administrador")]
        public async Task<ActionResult<usuario>> Get(int codigoUsuario)
        {
            try
            {
                usuario? objetoModel = new usuario();
                objetoModel = await _context.usuario.FirstOrDefaultAsync(x => x.codigoUsuario == codigoUsuario);

                if (objetoModel != null)
                {
                    objetoModel.correo = EncryptionHelper.Decrypt(objetoModel.correo);
                    return Ok(objetoModel);
                }
                else
                {
                    return BadRequest("Se obtuvo un NULL como respuesta en la búsqueda.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(String.Concat("Error: ", ex.Message));
            }
        }

        // POST api/<AgenciaController>
        [HttpPost]
        [AuthorizeRole("Administrador", "Administrador")]
        public async Task<ActionResult<string>> Post(usuario postModel)
        {
            try
            {
                // Hashing de la contraseña usando BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(postModel.clave);
                postModel.clave = hashedPassword;
                postModel.correo = EncryptionHelper.Encrypt(postModel.correo);
                _context.Add(postModel);
                await _context.SaveChangesAsync();
                return Ok("Usuario registrado correctamente.");
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.InnerException!.Message);
                return BadRequest(new { message = "Ocurrió un error al registrar el usuario. Intente nuevamente." });
            }
        }


        // PUT api/<AgenciaController>/5
        [HttpPut("{codigoUsuario:int}")]
        [AuthorizeRole("Administrador", "Administrador")]
        public async Task<ActionResult<string>> Put(usuario postModel, int codigoUsuario)
        {
            try
            {
                // Verificar si el usuario existe
                var existingUser = await _context.usuario.FindAsync(codigoUsuario);
                if (existingUser == null)
                {
                    return NotFound("El usuario no existe en el sistema.");
                }

                // Asegurarse de que el código de usuario coincida
                if (postModel.codigoUsuario != codigoUsuario)
                {
                    return BadRequest("El código de usuario proporcionado no coincide.");
                }

                // Cifrar el correo antes de actualizar
                if (!string.IsNullOrEmpty(postModel.correo))
                {
                    postModel.correo = EncryptionHelper.Encrypt(postModel.correo);
                }

                // Eliminar la clave del modelo recibido para evitar modificar la contraseña
                postModel.clave = existingUser.clave; // Mantener la contraseña existente

                // Actualizar el usuario
                _context.Entry(existingUser).CurrentValues.SetValues(postModel);
                await _context.SaveChangesAsync();
                return Ok("Se actualizó el registro.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] login loginRequest)
        {
            try
            {
                var user = await _context.usuario.FirstOrDefaultAsync(u => u.correo == EncryptionHelper.Encrypt(loginRequest.correo));
                if (user == null)
                {
                    return Unauthorized("El correo no está registrado."); 
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.clave, user.clave);
                if (!isPasswordValid)
                {
                    return Unauthorized("La contraseña es incorrecta."); 
                }

                var rol = await _context.rol.FirstOrDefaultAsync(r => r.codigoRol == user.codigoRol);
                if (rol == null)
                {
                    return BadRequest("Rol no encontrado."); 
                }

                var token = GenerateJwtToken(user, rol);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}"); // Mensaje de error
            }
        }


        private string GenerateJwtToken(usuario user, rol rol)
        {
            // Obtener la clave secreta desde appsettings.json
            var jwtSecret = _configuration["ApiSettings:jwtSecret"];
            var jwtExpiraMinutos = Convert.ToInt32(_configuration["ApiSettings:jwtExpiraMinutos"]);

            // Generar la clave de seguridad usando la clave secreta del archivo de configuración
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.codigoUsuario.ToString()),
        new Claim(ClaimTypes.Role, user.codigoRol.ToString()), 
        new Claim("NombreRol", rol.nombreRol), 
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7178",   
                audience: "https://localhost:7191", 
                claims: claims,
                expires: DateTime.Now.AddMinutes(jwtExpiraMinutos),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}
