using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class vw_agencias_acompanante
{
    public int? agencia_id { get; set; }

    public string? agencia_nombre { get; set; }

    public bool? agencia_verificada { get; set; }

    public long? total_acompanantes { get; set; }

    public long? acompanantes_verificados { get; set; }

    public long? acompanantes_disponibles { get; set; }
}
