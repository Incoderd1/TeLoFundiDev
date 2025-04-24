using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class failed_login_attempt
    {
        public int id { get; set; }
        public string email { get; set; }
        public string ip_address { get; set; }
        public DateTime attempt_time { get; set; }
    }
}
