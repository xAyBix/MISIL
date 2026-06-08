using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using FluentValidation;
using Misil.Application.Common.Exceptions;

namespace Misil.Api.Middleware;

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
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred";

        switch (exception)
        {
            case NotFoundException:
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
            case AuthenticationException:
                code = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case ValidationException ve:
                code = HttpStatusCode.BadRequest;
                message = "Validation failed";
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var body = new { error = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
