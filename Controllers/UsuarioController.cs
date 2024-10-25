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

namespace controlAcademico_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // Crear los claims que identifican al usuario, su rol, y su nombre de rol
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.codigoUsuario.ToString()),
        new Claim(ClaimTypes.Role, user.codigoRol.ToString()), // Rol del usuario
        new Claim("NombreRol", rol.nombreRol), // Aquí añadimos el nombre del rol
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Generar el token JWT con tiempo de expiración desde appsettings.json
            var token = new JwtSecurityToken(
                issuer: "https://localhost:7178",   // Aquí puedes cambiarlo a tu URL de emisor
                audience: "https://localhost:7191", // Aquí puedes cambiarlo a tu URL de audiencia
                claims: claims,
                expires: DateTime.Now.AddMinutes(jwtExpiraMinutos),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
