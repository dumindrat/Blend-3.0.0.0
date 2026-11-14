using Blend.BuisnessService.Token;
using System;

namespace Blend.BuisnessService.Authentication
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly IUserLookupService _userLookupService;

        public AuthenticationService(IUserLookupService userLookupService)
        {
            _userLookupService = userLookupService ?? throw new ArgumentNullException("userLookupService");
        }

        public AuthenticatedUser Authenticate(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _userLookupService.FindByUserName(userName);
            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return null;
            }

            var storedPassword = EncryptData.DecryptText(user.Password);
            if (!string.Equals(storedPassword, password, StringComparison.Ordinal))
            {
                return null;
            }

            return new AuthenticatedUser
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                PasswordExpireDate = user.PasswordExpireDate
            };
        }
    }
}
