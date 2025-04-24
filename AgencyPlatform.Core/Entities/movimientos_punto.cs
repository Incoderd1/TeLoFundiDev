using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class movimientos_punto
{
    public int id { get; set; }

    public int cliente_id { get; set; }

    public int cantidad { get; set; }

    public string tipo { get; set; } = null!;

    public string concepto { get; set; } = null!;

    public int saldo_anterior { get; set; }

    public int saldo_nuevo { get; set; }

    public DateTime? fecha { get; set; }

    public DateTime? created_at { get; set; }

    public virtual cliente cliente { get; set; } = null!;
}
