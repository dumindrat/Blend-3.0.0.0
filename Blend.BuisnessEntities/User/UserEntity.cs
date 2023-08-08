using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.BuisnessEntities
{
    public class UserEntity : BaseEntity
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsLogOff { get; set; }
        public string CertId { get; set; }
        public string Token { get; set; }
        public DateTime PasswordExpireDate { get; set; }

    }

    public class PasswordExpireInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime PasswordExpireDate { get; set; }
        public double ExpirationDays { get; set; }
        public bool IsRequireReset { get; set; }
    }
}
