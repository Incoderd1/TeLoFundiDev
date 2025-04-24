using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class vw_perfiles_populare
{
    public int? id { get; set; }

    public string? nombre_perfil { get; set; }

    public int? agencia_id { get; set; }

    public string? ciudad { get; set; }

    public decimal? tarifa_base { get; set; }

    public string? moneda { get; set; }

    public bool? esta_verificado { get; set; }

    public long? total_contactos { get; set; }
}
