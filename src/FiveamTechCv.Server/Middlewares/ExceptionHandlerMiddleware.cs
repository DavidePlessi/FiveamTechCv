using FiveamTechCv.Entities;
using System.Text.Json;

namespace FiveamTechCv.Server.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlerMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next.Invoke(httpContext);
        }
        catch (FiveamTechCvException exception)
        {
            httpContext.Response.StatusCode = exception.StatusCode;
            
            var body = new ExceptionResponse
            {
                ErrorCode = exception.ErrorCode,
                ErrorMessage = exception.ErrorMessage
            };

            var serializedBody = JsonSerializer.Serialize(body);
            await httpContext.Response.WriteAsync(serializedBody);
        }
        catch (Exception exception)
        {
            httpContext.Response.StatusCode = 500;
            var body = new ExceptionResponse()
            {
                ErrorCode = FiveamTechCvException.GenericError,
                ErrorMessage = "An error occurred while processing your request. Please try again later."
            };

            var serializedBody = JsonSerializer.Serialize(body);
            await httpContext.Response.WriteAsync(serializedBody);

            _logger.LogError(exception, "{ex}", exception);
        }
    }
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(
        this IApplicationBuilder builder
        )
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}