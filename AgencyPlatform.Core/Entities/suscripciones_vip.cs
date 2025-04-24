using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class suscripciones_vip
{
    public int id { get; set; }

    public int cliente_id { get; set; }

    public int membresia_id { get; set; }

    public DateTime fecha_inicio { get; set; }

    public DateTime fecha_fin { get; set; }

    public string? estado { get; set; }

    public string metodo_pago { get; set; } = null!;

    public string? referencia_pago { get; set; }

    public bool? es_renovacion_automatica { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual cliente cliente { get; set; } = null!;

    public virtual membresias_vip membresia { get; set; } = null!;
}
