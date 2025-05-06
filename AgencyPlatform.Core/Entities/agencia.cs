using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class agencia
{
    public int id { get; set; }

    public int usuario_id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public string? logo_url { get; set; }

    public string? sitio_web { get; set; }

    public string? direccion { get; set; }

    public string? ciudad { get; set; }

    public string? pais { get; set; }


    public bool? esta_verificada { get; set; }

    public DateTime? fecha_verificacion { get; set; }

    public decimal? comision_porcentaje { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
    public string? email { get; set; }  // Esta es la nueva columna
    public int puntos_gastados { get; set; }
    public int puntos_acumulados { get; set; }

    public string? stripe_account_id { get; set; } 



    public virtual ICollection<acompanante> acompanantes { get; set; } = new List<acompanante>();

    public virtual usuario usuario { get; set; } = null!;

    public virtual ICollection<verificacione> verificaciones { get; set; } = new List<verificacione>();
}
