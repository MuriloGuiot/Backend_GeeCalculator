namespace GEE_Calculator.Domain.Auth;

public sealed record ApiKeyValidationResult(
    Guid TenantId,
    string ClientName,
    string KeyPrefix);
