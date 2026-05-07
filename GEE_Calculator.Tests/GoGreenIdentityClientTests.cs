using System.Net;
using GEE_Calculator.Integration.GoGreen;
using Microsoft.Extensions.Options;

namespace GEE_Calculator.Tests;

public sealed class GoGreenIdentityClientTests
{
    [Fact]
    public async Task GetCurrentUserAsync_ShouldMapGoGreenPayload()
    {
        using var httpClient = new HttpClient(new StubHandler())
        {
            BaseAddress = new Uri("https://gogreen.example")
        };
        var client = new GoGreenIdentityClient(
            httpClient,
            Options.Create(new GoGreenIdentityOptions { CurrentUserEndpoint = "/me" }));

        var user = await client.GetCurrentUserAsync("token-123");

        Assert.NotNull(user);
        Assert.Equal("gogreen-user-1", user.Subject);
        Assert.Equal("murilo@gogreen.example", user.Email);
        Assert.Equal("11111111-1111-1111-1111-111111111111", user.TenantId);
        Assert.Contains("admin", user.Roles);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("token-123", request.Headers.Authorization?.Parameter);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "sub": "gogreen-user-1",
                      "email": "murilo@gogreen.example",
                      "name": "Murilo",
                      "tenant_id": "11111111-1111-1111-1111-111111111111",
                      "roles": ["admin"]
                    }
                    """)
            };

            return Task.FromResult(response);
        }
    }
}
