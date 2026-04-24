namespace GEE_Calculator.Application.Tenancy;

public sealed class TenantHeaderRequirementMiddleware(RequestDelegate next)
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/swagger",
        "/openapi",
        "/api/health"
    ];

    public async Task InvokeAsync(HttpContext context, ICurrentTenantAccessor tenantAccessor)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await next(context);
            return;
        }

        var currentTenant = tenantAccessor.GetCurrentTenant();

        if (!currentTenant.HasTenant)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "missing_tenant_header",
                message = $"The {TenantRequestHeaders.TenantId} header is required for this endpoint."
            });
            return;
        }

        await next(context);
    }

    private static bool ShouldSkip(PathString path)
    {
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ExcludedPrefixes.Any(prefix =>
            path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
