using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Exceptions
{
    public class InternalServerErrorApiException : ApiException
    {
        public InternalServerErrorApiException(string message)
            : base(500, message) // 500 es el código de estado para Internal Server Error
        {
        }
    }
}
