using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class membresias_vip
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public decimal precio_mensual { get; set; }

    public int puntos_mensuales { get; set; }

    public int descuento_anuncios { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
    public bool? esta_activa { get; set; }

    public virtual ICollection<suscripciones_vip> suscripciones_vips { get; set; } = new List<suscripciones_vip>();
}
