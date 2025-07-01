using eStreamChat.Interfaces;

namespace eStreamChat.Api.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IChatRoomStorage roomStorage)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<SkipAuthenticationAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var userId = roomStorage.GetUserIdByToken(token);
        if (userId == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.Items["UserId"] = userId;
        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipAuthenticationAttribute : Attribute
{
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseChatAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthenticationMiddleware>();
    }
}