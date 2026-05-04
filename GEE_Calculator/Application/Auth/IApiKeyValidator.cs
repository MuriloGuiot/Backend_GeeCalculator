namespace GEE_Calculator.Application.Auth;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult?> ValidateAsync(string apiKey, CancellationToken cancellationToken = default);
}
