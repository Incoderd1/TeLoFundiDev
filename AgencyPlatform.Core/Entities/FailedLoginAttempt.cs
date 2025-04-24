using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Entities
{
    public class FailedLoginAttempt
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string IpAddress { get; set; }
        public DateTime AttemptTime { get; set; }
    }
}
