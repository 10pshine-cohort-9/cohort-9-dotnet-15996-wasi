using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                // SERILOG YAHAN ERROR RECORD KAREGA
                _logger.LogError(ex, "System mein ek unexpected error aaya hai: {Message}", ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "System mein koi masla aa gaya hai. Hum isay theek karne ki koshish kar rahe hain.",
                DetailedError = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response);

            // SonarQube Fix: context.RequestAborted pass kiya hai
            return context.Response.WriteAsync(jsonResponse, context.RequestAborted);
        }
    }
}