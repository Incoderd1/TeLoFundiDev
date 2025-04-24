using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class acompanante
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public int? agencia_id { get; set; }

    public string nombre_perfil { get; set; } = null!;

    public string genero { get; set; } = null!;

    public int? edad { get; set; }

    public string? descripcion { get; set; }

    public int? altura { get; set; }

    public int? peso { get; set; }

    public string? ciudad { get; set; }

    public string? pais { get; set; }

    public string? idiomas { get; set; }

    public long? score_actividad { get; set; }


    public string? disponibilidad { get; set; }

    public decimal? tarifa_base { get; set; }

    public string? moneda { get; set; }

    public bool? esta_verificado { get; set; }

    public DateTime? fecha_verificacion { get; set; }

    public bool? esta_disponible { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    // Campos que faltan en tu clase acompanante:
    public string? telefono { get; set; }           // character varying(20)
    public string? whatsapp { get; set; }           // character varying(20)
    public string? email_contacto { get; set; }     // character varying(255)
    public bool? mostrar_telefono { get; set; }     // boolean
    public bool? mostrar_whatsapp { get; set; }     // boolean
    public bool? mostrar_email { get; set; }        // boolean


    public virtual ICollection<acompanante_categoria> acompanante_categoria { get; set; } = new List<acompanante_categoria>();

    public virtual agencia? agencia { get; set; }

    public virtual ICollection<anuncios_destacado> anuncios_destacados { get; set; } = new List<anuncios_destacado>();

    public virtual ICollection<contacto> contactos { get; set; } = new List<contacto>();

    public virtual ICollection<foto> fotos { get; set; } = new List<foto>();

    public virtual ICollection<servicio> servicios { get; set; } = new List<servicio>();

    public virtual usuario usuario { get; set; } = null!;

    public virtual verificacione? verificacione { get; set; }

    public virtual ICollection<visitas_perfil> visitas_perfils { get; set; } = new List<visitas_perfil>();
}
