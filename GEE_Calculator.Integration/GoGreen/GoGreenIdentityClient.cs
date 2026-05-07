using System.Net.Http.Headers;
using System.Text.Json;
using GEE_Calculator.Domain.Auth;
using Microsoft.Extensions.Options;

namespace GEE_Calculator.Integration.GoGreen;

public sealed class GoGreenIdentityClient(
    HttpClient httpClient,
    IOptions<GoGreenIdentityOptions> options) : IGoGreenIdentityClient
{
    public async Task<GoGreenUserSnapshot?> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var endpoint = options.Value.CurrentUserEndpoint;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        return new GoGreenUserSnapshot(
            Subject: ReadString(root, "subject", "sub", "id", "userId"),
            Email: ReadString(root, "email", "mail"),
            Name: ReadString(root, "name", "displayName", "preferred_username"),
            TenantId: ReadString(root, "tenantId", "tenant_id", "organizationId"),
            CompanyId: ReadString(root, "companyId", "company_id"),
            Roles: ReadRoles(root));
    }

    private static string? ReadString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static IReadOnlyCollection<string> ReadRoles(JsonElement root)
    {
        if (!root.TryGetProperty("roles", out var roles))
        {
            return [];
        }

        if (roles.ValueKind == JsonValueKind.Array)
        {
            return roles
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (roles.ValueKind == JsonValueKind.String)
        {
            var role = roles.GetString();
            return string.IsNullOrWhiteSpace(role) ? [] : [role];
        }

        return [];
    }
}
