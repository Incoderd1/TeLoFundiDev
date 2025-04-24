using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class refresh_token
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public string token { get; set; } = null!;

    public string? user_agent { get; set; }

    public string? ip_address { get; set; }

    public string? device_info { get; set; }

    public DateTime fecha_expiracion { get; set; }

    public bool? esta_revocado { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual usuario usuario { get; set; } = null!;
}
