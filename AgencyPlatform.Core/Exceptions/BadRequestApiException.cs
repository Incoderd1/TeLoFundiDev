using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Exceptions
{
    public class BadRequestApiException : ApiException
    {
        public BadRequestApiException(string message)
        : base(400, message) // 400 es el código de estado para Bad Request
        {
        }
    }
}
