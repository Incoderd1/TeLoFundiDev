using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Shared.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
        protected AppException(string message, Exception innerException)
            : base(message, innerException) { }



       
    }


    public class AccessDeniedException : AppException
    {
        public AccessDeniedException(string message) : base(message) { }
    }

    public class DuplicateException : AppException
    {
        public DuplicateException(string resourceType, string field, string value)
            : base($"Ya existe un {resourceType} con {field} = '{value}'") { }
    }
    public class BusinessRuleViolationException : AppException
    {
        public BusinessRuleViolationException(string message) : base(message) { }
    }

    /// <summary>
    /// Se lanza cuando se alcanza un límite del sistema (cuotas, límites diarios, etc.)
    /// </summary>
    public class LimitExceededException : AppException
    {
        public LimitExceededException(string message) : base(message) { }
    }

    /// <summary>
    /// Se lanza cuando falla una validación de datos
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message) { }

        public ValidationException(string field, string message)
            : base($"{field}: {message}") { }
    }

    /// <summary>
    /// Se lanza cuando falla una operación de pago o integración externa
    /// </summary>
    public class ExternalServiceException : AppException
    {
        public ExternalServiceException(string serviceName, string message)
            : base($"Error en servicio externo {serviceName}: {message}") { }
    }


}
