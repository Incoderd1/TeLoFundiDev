using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class categoria
{
    public int id { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<acompanante_categoria> acompanante_categoria { get; set; } = new List<acompanante_categoria>();
}
