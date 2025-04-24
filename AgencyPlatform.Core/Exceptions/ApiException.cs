using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Core.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public ApiException(int statusCode, string message, string errorCode = "")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    // Nuevas subclases de ApiException
    public class NotFoundException : ApiException
    {
        public NotFoundException(string resourceType, object id)
            : base((int)HttpStatusCode.NotFound,
                  $"{resourceType} con ID {id} no encontrado",
                  "RESOURCE_NOT_FOUND")
        {
        }
    }

    public class DuplicateEntityException : ApiException
    {
        public DuplicateEntityException(string resourceType, string field, string value)
            : base((int)HttpStatusCode.Conflict,
                  $"Ya existe un {resourceType} con {field} = '{value}'",
                  "DUPLICATE_ENTITY")
        {
        }
    }

    public class AccessDeniedException : ApiException
    {
        public AccessDeniedException(string message)
            : base((int)HttpStatusCode.Forbidden,
                  message,
                  "ACCESS_DENIED")
        {
        }
    }

    public class BusinessRuleViolationException : ApiException
    {
        public BusinessRuleViolationException(string message)
            : base((int)HttpStatusCode.BadRequest,
                  message,
                  "BUSINESS_RULE_VIOLATION")
        {
        }
    }

    public class LimitExceededException : ApiException
    {
        public LimitExceededException(string message)
            : base((int)HttpStatusCode.TooManyRequests,
                  message,
                  "LIMIT_EXCEEDED")
        {
        }
    }

    public class AppValidationException : ApiException
    {
        public AppValidationException(string message)
            : base((int)HttpStatusCode.BadRequest,
                  message,
                  "VALIDATION_FAILED")
        {
        }
    }

    public class ExternalServiceException : ApiException
    {
        public ExternalServiceException(string serviceName, string message)
            : base((int)HttpStatusCode.ServiceUnavailable,
                  $"Error en servicio externo {serviceName}: {message}",
                  "EXTERNAL_SERVICE_ERROR")
        {
        }
    }
}

