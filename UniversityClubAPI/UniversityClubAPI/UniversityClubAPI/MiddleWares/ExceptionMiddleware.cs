using System.Net;
using System.Text.Json;

namespace UniversityClubAPI.MiddleWares
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Path}", context.Request.Path);
                await WriteResponse(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found: {Path}", context.Request.Path);
                await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (InvalidOperationException ex)
            {

                _logger.LogWarning(ex, "Bad request: {Path}", context.Request.Path);
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {

                _logger.LogWarning(ex, "Argument error: {Path}", context.Request.Path);
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred");
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message,
                statusCode = (int)statusCode
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
        }
    }
}
