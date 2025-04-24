using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class intentos_login
{
    public int id { get; set; }

    public string email { get; set; } = null!;

    public string ip_address { get; set; } = null!;

    public int? intentos { get; set; }

    public DateTime? bloqueado_hasta { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
