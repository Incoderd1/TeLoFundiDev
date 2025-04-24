using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class contacto
{
    public int id { get; set; }

    public int acompanante_id { get; set; }

    public int? cliente_id { get; set; }

    public string tipo_contacto { get; set; } = null!;

    public DateTime? fecha_contacto { get; set; }

    public bool esta_registrado { get; set; }

    public string? ip_contacto { get; set; }

    public DateTime? created_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;

    public virtual cliente? cliente { get; set; }
}
