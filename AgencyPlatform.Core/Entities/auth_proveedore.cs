using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

/// <summary>
/// Proveedores de autenticación soportados (Google, Facebook, etc)
/// </summary>
public partial class auth_proveedore
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public bool? esta_activo { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<usuario_auth_externo> usuario_auth_externos { get; set; } = new List<usuario_auth_externo>();
}
