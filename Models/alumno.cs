using System;
using System.Collections.Generic;

namespace controlAcademico_web_api.Models;

public partial class alumno
{
    public int codigoAlumno { get; set; }

    public int? codigoUsuario { get; set; }

    public string nombreAlumno { get; set; } = null!;

    public string grado { get; set; } = null!;

    public byte estatus { get; set; }

    public virtual usuario? codigoUsuarioNavigation { get; set; }
}
