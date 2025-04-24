using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class cliente
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public string? nickname { get; set; }

    public bool? es_vip { get; set; }

    public DateTime? fecha_inicio_vip { get; set; }

    public DateTime? fecha_fin_vip { get; set; }

    public int? puntos_acumulados { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<compras_paquete> compras_paquetes { get; set; } = new List<compras_paquete>();

    public virtual ICollection<contacto> contactos { get; set; } = new List<contacto>();

    public virtual ICollection<cupones_cliente> cupones_clientes { get; set; } = new List<cupones_cliente>();

    public virtual ICollection<movimientos_punto> movimientos_puntos { get; set; } = new List<movimientos_punto>();

    public virtual ICollection<participantes_sorteo> participantes_sorteos { get; set; } = new List<participantes_sorteo>();

    public virtual ICollection<suscripciones_vip> suscripciones_vips { get; set; } = new List<suscripciones_vip>();

    public virtual usuario usuario { get; set; } = null!;

    public virtual ICollection<visitas_perfil> visitas_perfils { get; set; } = new List<visitas_perfil>();
}
