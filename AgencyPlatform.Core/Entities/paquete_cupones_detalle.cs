using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class paquete_cupones_detalle
{
    public int id { get; set; }

    public int paquete_id { get; set; }

    public int tipo_cupon_id { get; set; }

    public int cantidad { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual paquetes_cupone paquete { get; set; } = null!;

    public virtual tipos_cupone tipo_cupon { get; set; } = null!;
}
