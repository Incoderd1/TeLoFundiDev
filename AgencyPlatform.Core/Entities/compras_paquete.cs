using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class compras_paquete
{
    public int id { get; set; }

    public int cliente_id { get; set; }

    public int paquete_id { get; set; }

    public DateTime? fecha_compra { get; set; }

    public decimal monto_pagado { get; set; }

    public string metodo_pago { get; set; } = null!;

    public string? referencia_pago { get; set; }

    public string estado { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual cliente cliente { get; set; } = null!;

    public virtual paquetes_cupone paquete { get; set; } = null!;
}
