using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class tipos_cupone
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public decimal porcentaje_descuento { get; set; }

    public string? descripcion { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<cupones_cliente> cupones_clientes { get; set; } = new List<cupones_cliente>();

    public virtual ICollection<paquete_cupones_detalle> paquete_cupones_detalles { get; set; } = new List<paquete_cupones_detalle>();
}
