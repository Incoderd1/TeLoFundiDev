using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class servicio
{
    public int id { get; set; }

    public int acompanante_id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public decimal? precio { get; set; }

    public int? duracion_minutos { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;
}
