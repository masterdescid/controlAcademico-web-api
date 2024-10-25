using System;
using System.Collections.Generic;

namespace controlAcademico_web_api.Models;

public partial class usuario
{
    public int codigoUsuario { get; set; }

    public int? codigoRol { get; set; }

    public string correo { get; set; } = null!;

    public string clave { get; set; } = null!;

    public byte estatus { get; set; }

    public DateTime? fechaRegistro { get; set; }

    public virtual ICollection<alumno> alumno { get; set; } = new List<alumno>();

    public virtual rol? codigoRolNavigation { get; set; }

    public virtual ICollection<maestro> maestro { get; set; } = new List<maestro>();
}
