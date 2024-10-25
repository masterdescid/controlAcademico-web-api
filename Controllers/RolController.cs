using controlAcademico_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace controlAcademico_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/<AgenciaController>
        [HttpGet]
        [AuthorizeRole("Administrador")]
        public async Task<ActionResult<List<rol>>> Get()
        {
            try
            {
                List<rol>? objetoModel = new List<rol>();
                objetoModel = await _context.rol.Where(x => x.estatus == 1 || x.estatus == 2).ToListAsync();

                if (objetoModel != null)
                {
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
        [HttpGet("{codigoRol:int}")]
        [AuthorizeRole("Administrador")]
        public async Task<ActionResult<rol>> Get(int codigoRol)
        {
            try
            {
                rol? objetoModel = new rol();
                objetoModel = await _context.rol.FirstOrDefaultAsync(x => x.codigoRol == codigoRol);

                if (objetoModel != null)
                {
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
        [AuthorizeRole("Administrador")]
        public async Task<ActionResult<string>> Post(rol postModel)
        {
            try
            {

                _context.Add(postModel);
                var resultado = await _context.SaveChangesAsync();
                return Ok("Se guardó el registro.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException!.Message);
            }
        }

        // PUT api/<AgenciaController>/5
        [HttpPut("{codigoRol:int}")]
        [AuthorizeRole("Administrador")]
        public async Task<ActionResult<string>> Put(rol postModel, int codigoRol)
        {
            try
            {

                if (postModel.codigoRol != codigoRol)
                {
                    return BadRequest("El registro no existe en el sistema.");
                }

                _context.Update(postModel);
                await _context.SaveChangesAsync();
                return Ok("Se actualizó el registro.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException!.Message);
            }
        }
    }
}
