using System.Security.Cryptography;
using System.Text;
using GEE_Calculator.Application.Auth;
using GEE_Calculator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Auth;

public sealed class ApiKeyValidator(GeeCalculatorDbContext dbContext) : IApiKeyValidator
{
    public async Task<ApiKeyValidationResult?> ValidateAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var normalizedApiKey = apiKey.Trim();
        var keyHash = ComputeSha256(normalizedApiKey);

        var client = await dbContext.ApiClients
            .AsNoTracking()
            .Where(item => item.IsActive && item.RevokedAt == null && item.KeyHash == keyHash)
            .SingleOrDefaultAsync(cancellationToken);

        return client is null
            ? null
            : new ApiKeyValidationResult(client.TenantId, client.Name, client.KeyPrefix);
    }

    public static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
