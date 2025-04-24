using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

/// <summary>
/// Asociación entre usuarios y sus cuentas en proveedores externos
/// </summary>
public partial class usuario_auth_externo
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public int proveedor_id { get; set; }

    public string proveedor_user_id { get; set; } = null!;

    public bool? email_verificado { get; set; }

    public string? datos_extra { get; set; }

    public DateTime? fecha_ultima_autenticacion { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual auth_proveedore proveedor { get; set; } = null!;

    public virtual usuario usuario { get; set; } = null!;
}
