using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class verificacione
{
    public int id { get; set; }

    public int agencia_id { get; set; }

    public int acompanante_id { get; set; }

    public DateTime? fecha_verificacion { get; set; }

    public decimal? monto_cobrado { get; set; }

    public string? estado { get; set; }

    public string? observaciones { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;

    public virtual agencia agencia { get; set; } = null!;
}
