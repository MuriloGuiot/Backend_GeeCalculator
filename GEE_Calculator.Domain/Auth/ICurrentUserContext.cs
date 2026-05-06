namespace GEE_Calculator.Domain.Auth;

public interface ICurrentUserContext
{
    CurrentUserSnapshot GetCurrentUser();
}
