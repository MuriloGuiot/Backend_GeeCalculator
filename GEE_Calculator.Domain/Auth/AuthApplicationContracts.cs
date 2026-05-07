namespace GEE_Calculator.Domain.Auth;

public sealed record GoGreenUserSnapshot(
    string? Subject,
    string? Email,
    string? Name,
    string? TenantId,
    string? CompanyId,
    IReadOnlyCollection<string> Roles);

public interface IAuthApplicationService
{
    Task<CurrentUserSnapshot> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public interface IGoGreenIdentityClient
{
    Task<GoGreenUserSnapshot?> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default);
}
