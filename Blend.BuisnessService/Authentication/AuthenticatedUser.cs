using System;

namespace Blend.BuisnessService.Authentication
{
    public sealed class AuthenticatedUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime PasswordExpireDate { get; set; }
    }
}
