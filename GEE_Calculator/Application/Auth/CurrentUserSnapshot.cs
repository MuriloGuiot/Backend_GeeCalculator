namespace GEE_Calculator.Application.Auth;

public sealed record CurrentUserSnapshot(
    string? Subject,
    string? Email,
    string? Name,
    string? TenantId,
    string? CompanyId,
    IReadOnlyCollection<string> Roles,
    bool IsAuthenticated);
