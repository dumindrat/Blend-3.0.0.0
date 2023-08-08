using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blend.Data.Request;

namespace Blend.Data.User
{
    public class SystemUser : BaseEntity
    {
        public int UserId { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }
    }

    public class SystemUserRequestDto : RequestDto
    {
        public SystemUser user { get; set; }
    }
}
