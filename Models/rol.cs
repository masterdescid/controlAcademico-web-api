using System;
using System.Collections.Generic;

namespace controlAcademico_web_api.Models;

public partial class rol
{
    public int codigoRol { get; set; }

    public string nombreRol { get; set; } = null!;

    public byte estatus { get; set; }

    public virtual ICollection<usuario> usuario { get; set; } = new List<usuario>();
}
