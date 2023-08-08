using Blend.BuisnessEntities;
using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessEntities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.AccessRights.Roles
{
    public interface IRoleService
    {
        IList<RoleEntity> GetRoles(string searchBy);
        RoleEntity GetRoleById(int roleId);
        IList<RoleAccessLevel> GetRoleAccessLevels(int roleId);
        bool CreateRole(RoleEntity obj);
        bool UpdateRole(RoleEntity obj);
        bool DeleteRole(RoleEntity obj);
        bool UpdateRoleAccessLevels(RoleAccessLevel obj);
        bool AddUser(UserRole obj);
        bool RemoveUser(UserRole obj);

    }
}
