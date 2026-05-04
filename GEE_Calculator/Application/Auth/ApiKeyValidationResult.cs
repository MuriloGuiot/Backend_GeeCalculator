namespace GEE_Calculator.Application.Auth;

public sealed record ApiKeyValidationResult(
    Guid TenantId,
    string ClientName,
    string KeyPrefix);
