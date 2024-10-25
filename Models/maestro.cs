using System;
using System.Collections.Generic;

namespace controlAcademico_web_api.Models;

public partial class maestro
{
    public int codigoMaestro { get; set; }

    public int? codigoUsuario { get; set; }

    public string nombreMaestro { get; set; } = null!;

    public string asignatura { get; set; } = null!;

    public byte estatus { get; set; }

    public virtual usuario? codigoUsuarioNavigation { get; set; }
}
