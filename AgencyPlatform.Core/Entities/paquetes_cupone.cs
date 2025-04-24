using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class paquetes_cupone
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public decimal precio { get; set; }

    public int puntos_otorgados { get; set; }

    public bool? incluye_sorteo { get; set; }

    public bool? activo { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<compras_paquete> compras_paquetes { get; set; } = new List<compras_paquete>();

    public virtual ICollection<paquete_cupones_detalle> paquete_cupones_detalles { get; set; } = new List<paquete_cupones_detalle>();
}
