using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.AccessRights.Role
{
    public class RoleEntity : BaseEntity
    {
        public int RoleId { get; set; }
        public string RoleDescription { get; set; }
    }

    public class RoleAccessLevel : BaseEntity
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string AccessLevel { get; set; }
    }
}
