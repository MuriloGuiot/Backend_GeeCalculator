using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.WebApi.Tenancy;

public sealed class TenantHeaderRequirementMiddleware(RequestDelegate next)
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/swagger",
        "/openapi",
        "/api/health"
    ];

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentTenantAccessor tenantAccessor,
        IApiKeyValidator apiKeyValidator)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await next(context);
            return;
        }

        var apiKey = context.Request.Headers[TenantRequestHeaders.ApiKey].ToString();

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var validatedApiKey = await apiKeyValidator.ValidateAsync(apiKey, context.RequestAborted);

            if (validatedApiKey is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_api_key",
                    message = $"The {TenantRequestHeaders.ApiKey} header is invalid or inactive."
                });
                return;
            }

            var currentTenantHeader = context.Request.Headers[TenantRequestHeaders.TenantId].ToString();

            if (string.IsNullOrWhiteSpace(currentTenantHeader))
            {
                context.Request.Headers[TenantRequestHeaders.TenantId] = validatedApiKey.TenantId.ToString();
            }
            else if (!string.Equals(currentTenantHeader, validatedApiKey.TenantId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "tenant_api_key_mismatch",
                    message = $"The {TenantRequestHeaders.TenantId} header does not match the informed API key."
                });
                return;
            }
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
