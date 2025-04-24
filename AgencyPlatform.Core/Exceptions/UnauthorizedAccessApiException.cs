using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Exceptions
{
    public class UnauthorizedAccessApiException : ApiException
    {
        public UnauthorizedAccessApiException(string message)
            : base(401, message) // 401 es el código de estado para Unauthorized
        {
        }
    }
}
