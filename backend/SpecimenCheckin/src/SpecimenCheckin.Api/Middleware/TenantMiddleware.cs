namespace SpecimenCheckin.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow public endpoints (tenant listing) without tenant context
        if (context.Request.Path.StartsWithSegments("/api/tenants", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(
                """{"type":"https://errors/tenant-required","title":"Tenant header required","status":400,"detail":"X-Tenant-Id header is required for all requests"}""");
            return;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(
                """{"type":"https://errors/user-required","title":"User header required","status":400,"detail":"X-User-Id header is required for all requests"}""");
            return;
        }

        context.Items["TenantId"] = tenantId;
        context.Items["UserId"] = userId;

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}

public static class HttpContextTenantExtensions
{
    public static string GetTenantId(this HttpContext context)
    {
        return context.Items["TenantId"] as string
            ?? throw new InvalidOperationException("Tenant ID not found in request context");
    }

    public static string GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] as string
            ?? throw new InvalidOperationException("User ID not found in request context");
    }
}
