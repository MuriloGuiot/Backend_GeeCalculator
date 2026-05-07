using GEE_Calculator.Domain.Auth;

namespace GEE_Calculator.Application.Auth;

public sealed class AuthApplicationService(
    ICurrentUserContext currentUserContext,
    IGoGreenIdentityClient goGreenIdentityClient,
    IAccessTokenReader accessTokenReader) : IAuthApplicationService
{
    public async Task<CurrentUserSnapshot> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var localUser = currentUserContext.GetCurrentUser();
        var accessToken = accessTokenReader.GetBearerToken();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return localUser;
        }

        var goGreenUser = await goGreenIdentityClient.GetCurrentUserAsync(accessToken, cancellationToken);

        if (goGreenUser is null)
        {
            return localUser;
        }

        return new CurrentUserSnapshot(
            Subject: goGreenUser.Subject ?? localUser.Subject,
            Email: goGreenUser.Email ?? localUser.Email,
            Name: goGreenUser.Name ?? localUser.Name,
            TenantId: goGreenUser.TenantId ?? localUser.TenantId,
            CompanyId: goGreenUser.CompanyId ?? localUser.CompanyId,
            Roles: goGreenUser.Roles.Count > 0 ? goGreenUser.Roles : localUser.Roles,
            IsAuthenticated: true);
    }
}

public interface IAccessTokenReader
{
    string? GetBearerToken();
}
