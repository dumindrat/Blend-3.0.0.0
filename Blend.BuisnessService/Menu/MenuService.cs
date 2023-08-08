using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessEntities.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Menu
{
    public class MenuService : BaseManagement<MenuEntity>, IMenuService
    {
        public IEnumerable<BuisnessEntities.Menu.MenuEntity> GetMenuByUserName(string userName)
        {
            try
            {
                var mapper = new DataMapper<MenuEntity>("GetMenuItems", new MenuEntity { UserName = userName });

                var result = mapper.ObjectList();
                logger.LogWrite("Get by menu items :" + result.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetMenuByUserName fails. Error " + ex.Message);
                throw ex;
            }

        }


        public RoleAccessLevel GetMenuById(int menuId, int roleId)
        {
            try
            {
                var mapper = new DataMapper<RoleAccessLevel>("GetMenuById", new Hashtable() { { "MenuId", menuId }, { "RoleId", roleId } });

                var result = mapper.Object();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetMenuById fails. Error " + ex.Message);
                throw ex;
            }

        }


        public IEnumerable<MenuEntity> GetMenuByUserNameAndMainModule(string userName, int mainModule)
        {
            try
            {
                var mapper = new DataMapper<MenuEntity>("GetMenuItems", new MenuEntity { UserName = userName, MainModule = mainModule });

                var result = mapper.ObjectList();
                logger.LogWrite("Get by menu items :" + result.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetMenuByUserName fails. Error " + ex.Message);
                throw ex;
            }
        }
    }
}
