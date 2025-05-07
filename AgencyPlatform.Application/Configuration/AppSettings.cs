using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Configuration
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int TokenExpirationHours { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
