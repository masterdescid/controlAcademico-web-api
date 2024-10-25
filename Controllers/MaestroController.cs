using controlAcademico_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace controlAcademico_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaestroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaestroController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/<AgenciaController>
        [HttpGet]
        [AuthorizeRole("Maestro","Administrador")]
        public async Task<ActionResult<List<usuario>>> Get()
        {
            try
            {
                List<maestro>? objetoModel = new List<maestro>();
                objetoModel = await _context.maestro.Where(x => x.estatus == 1 || x.estatus == 2).ToListAsync();

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
        [HttpGet("{codigoMaestro:int}")]
        [AuthorizeRole("Maestro", "Administrador")]
        public async Task<ActionResult<maestro>> Get(int codigoMaestro)
        {
            try
            {
                maestro? objetoModel = new maestro();
                objetoModel = await _context.maestro.FirstOrDefaultAsync(x => x.codigoMaestro == codigoMaestro);

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
        [AuthorizeRole("Maestro", "Administrador")]
        public async Task<ActionResult<string>> Post(maestro postModel)
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
        [HttpPut("{codigoMaestro:int}")]
        [AuthorizeRole("Maestro", "Administrador")]
        public async Task<ActionResult<string>> Put(maestro postModel, int codigoMaestro)
        {
            try
            {

                if (postModel.codigoMaestro != codigoMaestro)
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
