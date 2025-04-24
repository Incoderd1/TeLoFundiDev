using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Exceptions
{
    public class NotFoundApiException : ApiException
    {
        public NotFoundApiException(string message)
            : base(404, message) // 404 es el código de estado para Not Found
        {
        }
    }
}
