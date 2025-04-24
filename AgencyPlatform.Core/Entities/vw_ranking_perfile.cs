using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class vw_ranking_perfile
{
    public int? id { get; set; }

    public string? nombre_perfil { get; set; }

    public int? agencia_id { get; set; }

    public string? ciudad { get; set; }

    public bool? esta_verificado { get; set; }

    public long? total_visitas { get; set; }

    public long? total_contactos { get; set; }

    public long? score_actividad { get; set; }
}
