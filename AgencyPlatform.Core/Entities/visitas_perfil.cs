using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class visitas_perfil
{
    public int id { get; set; }

    public int acompanante_id { get; set; }

    public int? cliente_id { get; set; }

    public string? ip_visitante { get; set; }

    public string? user_agent { get; set; }

    public DateTime? fecha_visita { get; set; }

    public int? duracion_segundos { get; set; }

    public DateTime? created_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;

    public virtual cliente? cliente { get; set; }
}
