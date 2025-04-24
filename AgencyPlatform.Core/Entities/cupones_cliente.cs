using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class cupones_cliente
{
    public int id { get; set; }

    public int cliente_id { get; set; }

    public int tipo_cupon_id { get; set; }

    public string codigo { get; set; } = null!;

    public DateTime? fecha_creacion { get; set; }

    public DateTime fecha_expiracion { get; set; }

    public bool? esta_usado { get; set; }

    public DateTime? fecha_uso { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<anuncios_destacado> anuncios_destacados { get; set; } = new List<anuncios_destacado>();

    public virtual cliente cliente { get; set; } = null!;

    public virtual tipos_cupone tipo_cupon { get; set; } = null!;
}
