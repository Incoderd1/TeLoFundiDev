using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class participantes_sorteo
{
    public int id { get; set; }

    public int sorteo_id { get; set; }

    public int cliente_id { get; set; }

    public DateTime? fecha_participacion { get; set; }

    public bool? es_ganador { get; set; }

    public DateTime? created_at { get; set; }

    public virtual cliente cliente { get; set; } = null!;

    public virtual sorteo sorteo { get; set; } = null!;
}
