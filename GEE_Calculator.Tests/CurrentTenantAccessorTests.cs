using System.Security.Claims;
using GEE_Calculator.Application.Tenancy;
using Microsoft.AspNetCore.Http;

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
        });

        var snapshot = accessor.GetCurrentTenant();

        Assert.True(snapshot.IsResolved);
        Assert.Equal("11111111-1111-1111-1111-111111111111", snapshot.TenantId);
        Assert.Equal("22222222-2222-2222-2222-222222222222", snapshot.CompanyId);
    }
}
