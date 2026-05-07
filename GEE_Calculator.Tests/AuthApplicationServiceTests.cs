using GEE_Calculator.Application.Auth;
using GEE_Calculator.Domain.Auth;

namespace GEE_Calculator.Tests;

public sealed class AuthApplicationServiceTests
{
    [Fact]
    public async Task GetCurrentUserAsync_ShouldEnrichLocalUserWithGoGreenIdentity()
    {
        var service = new AuthApplicationService(
            new FixedCurrentUserContext(),
            new FixedGoGreenIdentityClient(),
            new FixedAccessTokenReader("token-123"));

        var user = await service.GetCurrentUserAsync();

        Assert.Equal("gogreen-user-1", user.Subject);
        Assert.Equal("murilo@gogreen.example", user.Email);
        Assert.Equal("GoGreen Admin", user.Roles.Single());
        Assert.True(user.IsAuthenticated);
    }

    private sealed class FixedCurrentUserContext : ICurrentUserContext
    {
        public CurrentUserSnapshot GetCurrentUser()
        {
            return new CurrentUserSnapshot(
                Subject: "local-user",
                Email: null,
                Name: "Local User",
                TenantId: "11111111-1111-1111-1111-111111111111",
                CompanyId: null,
                Roles: [],
                IsAuthenticated: true);
        }
    }

    private sealed class FixedGoGreenIdentityClient : IGoGreenIdentityClient
    {
        public Task<GoGreenUserSnapshot?> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<GoGreenUserSnapshot?>(new GoGreenUserSnapshot(
                Subject: "gogreen-user-1",
                Email: "murilo@gogreen.example",
                Name: "Murilo",
                TenantId: null,
                CompanyId: null,
                Roles: ["GoGreen Admin"]));
        }
    }

    private sealed class FixedAccessTokenReader(string? token) : IAccessTokenReader
    {
        public string? GetBearerToken() => token;
    }
}
