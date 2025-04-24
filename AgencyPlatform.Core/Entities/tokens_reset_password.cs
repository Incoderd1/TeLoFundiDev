using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class tokens_reset_password
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public string token { get; set; } = null!;

    public DateTime fecha_expiracion { get; set; }

    public bool esta_usado { get; set; }

    public DateTime? created_at { get; set; }

    public virtual usuario usuario { get; set; } = null!;
}
