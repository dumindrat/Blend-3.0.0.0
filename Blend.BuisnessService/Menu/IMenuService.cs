using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessEntities.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Menu
{
    public interface IMenuService
    {
        IEnumerable<MenuEntity> GetMenuByUserName(string userName);
        IEnumerable<MenuEntity> GetMenuByUserNameAndMainModule(string userName,int mainModule );
        RoleAccessLevel GetMenuById(int menuId, int roleId);

    }
}
