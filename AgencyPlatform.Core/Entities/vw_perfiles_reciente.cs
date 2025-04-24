using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class vw_perfiles_reciente
{
    public int? id { get; set; }

    public int? usuario_id { get; set; }

    public int? agencia_id { get; set; }

    public string? nombre_perfil { get; set; }

    public string? genero { get; set; }

    public int? edad { get; set; }

    public string? descripcion { get; set; }

    public int? altura { get; set; }

    public int? peso { get; set; }

    public string? ciudad { get; set; }

    public string? pais { get; set; }

    public string? idiomas { get; set; }

    public string? disponibilidad { get; set; }

    public decimal? tarifa_base { get; set; }

    public string? moneda { get; set; }

    public bool? esta_verificado { get; set; }

    public DateTime? fecha_verificacion { get; set; }

    public bool? esta_disponible { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public string? foto_principal { get; set; }
}
