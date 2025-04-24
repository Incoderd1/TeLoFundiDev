using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Configuration
{
    public class ClienteSettings
    {
        public int CuponesValidezMeses { get; set; } = 3;
        public decimal BonusVipPorcentaje { get; set; } = 20;
        public int SuscripcionDuracionMeses { get; set; } = 1;
    }
}
