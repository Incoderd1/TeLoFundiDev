using System;
using System.Collections.Generic;

namespace AgencyPlatform.Core.Entities;

public partial class acompanante_categoria
{
    public int id { get; set; }

    public int acompanante_id { get; set; }

    public int categoria_id { get; set; }

    public DateTime? created_at { get; set; }

    public virtual acompanante acompanante { get; set; } = null!;

    public virtual categoria categoria { get; set; } = null!;
}
