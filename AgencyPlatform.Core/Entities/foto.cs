using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class foto
{
    public int id { get; set; }
    public int acompanante_id { get; set; }
    public string url { get; set; } = null!;
    public bool? es_principal { get; set; }
    public int? orden { get; set; }
    public bool verificada { get; set; }    // no-nullable, OK
    public DateTime fecha_verificacion { get; set; }    // no-nullable, OK
    public DateTime? created_at { get; set; }    // <-- en tu ModelBuilder lo marcaste IsRequired
    public DateTime? updated_at { get; set; }    // <-- idem
    public virtual acompanante acompanante { get; set; } = null!;
}

