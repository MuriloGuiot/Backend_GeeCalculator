namespace GEE_Calculator.Application.Auth;

public interface ICurrentUserContext
{
    CurrentUserSnapshot GetCurrentUser();
}
