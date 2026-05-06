namespace GEE_Calculator.Domain.Auth;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult?> ValidateAsync(string apiKey, CancellationToken cancellationToken = default);
}
