using controlAcademico_web_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace controlAcademico_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlumnoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AlumnoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/<AgenciaControllers>
        [HttpGet]
        [AuthorizeRole("Alumno","Administrador")]
        public async Task<ActionResult<List<usuario>>> Get()
        {
            try
            {
                List<alumno>? objetoModel = new List<alumno>();
                objetoModel = await _context.alumno.Where(x => x.estatus == 1 || x.estatus == 2).ToListAsync();

                if (objetoModel != null)
                {
                    return Ok(objetoModel);
                }
                else
                {
                    return BadRequest("Se obtuvo un NULL como respuesta en la búsqueda.........");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(String.Concat("Error: ", ex.Message));
            }
        }

        // GET api/<AgenciaController>/5
        [HttpGet("{codigoAlumno:int}")]
        [AuthorizeRole("Alumno", "Administrador")]
        public async Task<ActionResult<usuario>> Get(int codigoAlumno)
        {
            try
            {
                alumno? objetoModel = new alumno();
                objetoModel = await _context.alumno.FirstOrDefaultAsync(x => x.codigoAlumno == codigoAlumno);

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
        [AuthorizeRole("Alumno", "Administrador")]
        public async Task<ActionResult<string>> Post(alumno postModel)
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
        [HttpPut("{codigoAlumno:int}")]
        [AuthorizeRole("Alumno", "Administrador")]
        public async Task<ActionResult<string>> Put(alumno postModel, int codigoAlumno)
        {
            try
            {

                if (postModel.codigoAlumno != codigoAlumno)
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
