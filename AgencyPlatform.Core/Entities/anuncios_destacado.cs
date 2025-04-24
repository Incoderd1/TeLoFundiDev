using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class anuncios_destacado
{
    public int id { get; set; }

    public int acompanante_id { get; set; }

    public DateTime fecha_inicio { get; set; }

    public DateTime fecha_fin { get; set; }

    public string tipo { get; set; } = null!;

    public decimal monto_pagado { get; set; }

    public int? cupon_id { get; set; }

    public bool? esta_activo { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;

    public virtual cupones_cliente? cupon { get; set; }
}
