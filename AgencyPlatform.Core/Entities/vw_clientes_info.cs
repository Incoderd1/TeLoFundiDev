using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class vw_clientes_info
{
    public int? id { get; set; }

    public int? usuario_id { get; set; }

    public string? email { get; set; }

    public string? nickname { get; set; }

    public bool? es_vip { get; set; }

    public int? puntos_acumulados { get; set; }

    public long? cupones_disponibles { get; set; }
}
