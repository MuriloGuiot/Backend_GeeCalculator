using System.Security.Claims;
using GEE_Calculator.WebApi.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace GEE_Calculator.Tests;

public sealed class CurrentTenantAccessorTests
{
    [Fact]
    public void GetCurrentTenant_ShouldResolveTenantAndCompanyFromClaimsWhenHeadersAreMissing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("tenant_id", "11111111-1111-1111-1111-111111111111"),
            new Claim("company_id", "22222222-2222-2222-2222-222222222222")
        ], authenticationType: "Bearer"));

        var accessor = new CurrentTenantAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext
        }, Options.Create(new TenancyOptions()));

        var snapshot = accessor.GetCurrentTenant();

        Assert.True(snapshot.IsResolved);
        Assert.Equal("11111111-1111-1111-1111-111111111111", snapshot.TenantId);
        Assert.Equal("22222222-2222-2222-2222-222222222222", snapshot.CompanyId);
    }

    [Fact]
    public void GetCurrentTenant_ShouldIgnoreTenantHeaderWhenFallbackIsDisabled()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "11111111-1111-1111-1111-111111111111";

        var accessor = new CurrentTenantAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext
        }, Options.Create(new TenancyOptions { AllowTenantHeaderFallback = false }));

        var snapshot = accessor.GetCurrentTenant();

        Assert.False(snapshot.HasTenant);
    }

    [Fact]
    public void GetCurrentTenant_ShouldUseTenantHeaderWhenFallbackIsEnabled()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "11111111-1111-1111-1111-111111111111";

        var accessor = new CurrentTenantAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext
        }, Options.Create(new TenancyOptions { AllowTenantHeaderFallback = true }));

        var snapshot = accessor.GetCurrentTenant();

        Assert.True(snapshot.HasTenant);
        Assert.Equal("11111111-1111-1111-1111-111111111111", snapshot.TenantId);
    }
}
