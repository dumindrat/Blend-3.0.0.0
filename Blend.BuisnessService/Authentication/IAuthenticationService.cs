namespace Blend.BuisnessService.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticatedUser Authenticate(string userName, string password);
    }
}
