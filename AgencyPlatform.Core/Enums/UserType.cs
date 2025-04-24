using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Enums
{
    public enum UserType
    {
        Admin = 1,
        Agency = 2,
        Escort = 3,   // o Acompanante si prefieres mantener el término en español
        Client = 4,
        VipClient = 5
    }
}
