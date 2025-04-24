using AgencyPlatform.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgencyPlatform.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errorCode) = exception switch
            {
                ApiException apiEx => (apiEx.StatusCode, apiEx.Message, apiEx.ErrorCode),
                _ => ((int)HttpStatusCode.InternalServerError, "Error interno del servidor.", "INTERNAL_SERVER_ERROR")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                status = statusCode,
                message = message,
                errorCode = errorCode,
                details = exception.InnerException?.Message
            };

            var json = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(json);
        }
    }

}
