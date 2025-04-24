using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class acciones_punto
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public int puntos { get; set; }

    public int? limite_diario { get; set; }

    public bool? esta_activa { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
