using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class sorteo
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public DateTime fecha_inicio { get; set; }

    public DateTime fecha_fin { get; set; }

    public string premio { get; set; } = null!;

    public bool? esta_activo { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<participantes_sorteo> participantes_sorteos { get; set; } = new List<participantes_sorteo>();
}
