using System.Net;
using System.Text.Json;
using eStreamChat.Api.Models.Responses;
using FluentValidation;

namespace eStreamChat.Api.Middleware;

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
            _logger.LogError(ex, "An error occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ApiErrorResponse response;
        HttpStatusCode statusCode;

        if (exception is ValidationException validationEx)
        {
            response = new ApiErrorResponse
            {
                Error = "Validation failed",
                Details = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))
            };
            statusCode = HttpStatusCode.BadRequest;
        }
        else if (exception is UnauthorizedAccessException)
        {
            response = new ApiErrorResponse
            {
                Error = "Unauthorized"
            };
            statusCode = HttpStatusCode.Unauthorized;
        }
        else if (exception is KeyNotFoundException)
        {
            response = new ApiErrorResponse
            {
                Error = "Resource not found"
            };
            statusCode = HttpStatusCode.NotFound;
        }
        else
        {
            response = new ApiErrorResponse
            {
                Error = "An unexpected error occurred"
            };
            statusCode = HttpStatusCode.InternalServerError;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}