using AgencyPlatform.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = exception.Message;

            switch (exception)
            {
                case NotFoundException: code = HttpStatusCode.NotFound; break;
                case UnauthorizedAccessException: code = HttpStatusCode.Forbidden; break;
                case ValidationException: code = HttpStatusCode.BadRequest; break;
                case BusinessRuleViolationException: code = HttpStatusCode.BadRequest; break;
                case DuplicateEntityException: code = HttpStatusCode.Conflict; break;
            }

            var problem = new
            {
                title = code.ToString(),
                status = (int)code,
                detail = message
            };

            var payload = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(payload);
        }

    }
}
