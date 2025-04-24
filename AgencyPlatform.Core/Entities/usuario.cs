using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class usuario
{
    public int id { get; set; }

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string? telefono { get; set; }

    public int rol_id { get; set; }

    public DateTime? fecha_registro { get; set; }

    public DateTime? ultimo_acceso { get; set; }

    public bool? esta_activo { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public string? provider { get; set; }

    public bool? password_required { get; set; }

    public virtual acompanante? acompanante { get; set; }

    public virtual agencia? agencia { get; set; }

    public virtual cliente? cliente { get; set; }

    public virtual ICollection<refresh_token> refresh_tokens { get; set; } = new List<refresh_token>();

    public virtual role rol { get; set; } = null!;

    public virtual ICollection<tokens_reset_password> tokens_reset_passwords { get; set; } = new List<tokens_reset_password>();

    public virtual ICollection<usuario_auth_externo> usuario_auth_externos { get; set; } = new List<usuario_auth_externo>();
}
