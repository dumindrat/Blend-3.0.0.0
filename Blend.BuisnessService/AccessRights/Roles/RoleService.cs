using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessEntities.User;
using Blend.BuisnessService.Menu;
using Blend.BuisnessService.User;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.AccessRights.Roles
{
    public class RoleService : BaseManagement<RoleEntity>, IRoleService
    {
        private IMenuService _menuService;

        public RoleService(IMenuService menuService)
        {
            _menuService = menuService;
        }
        public IList<RoleEntity> GetRoles(string searchBy)
        {
            try
            {
                var search = searchBy == "all" ? "" : searchBy;
                var mapper = new DataMapper<RoleEntity>("GetRoles", new Hashtable() { { "RoleDescription", search } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public RoleEntity GetRoleById(int roleId)
        {
            throw new NotImplementedException();
        }

        public bool CreateRole(RoleEntity obj)
        {
            try
            {
                dataOperation.CommandText("auth_RoleInsert");
                dataOperation.AddParam("role", obj.RoleDescription);
                dataOperation.AddParam("user", obj.User);
                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool UpdateRole(RoleEntity obj)
        {
            try
            {
                dataOperation.CommandText("auth_RoleUpdate");
                dataOperation.AddParam("roleids", obj.RoleId);
                dataOperation.AddParam("roledesc", obj.RoleDescription);
                dataOperation.AddParam("user", obj.User);

                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool DeleteRole(RoleEntity obj)
        {
            try
            {
                dataOperation.CommandText("auth_RoleDelete");
                dataOperation.AddParam("roleids", obj.RoleId);
                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public IList<RoleAccessLevel> GetRoleAccessLevels(int roleId)
        {
            try
            {
                var mapper = new DataMapper<RoleAccessLevel>("GetRoleAccessLevels", new Hashtable() { { "RoleId", roleId } });
                var result = mapper.ObjectList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public bool UpdateRoleAccessLevels(RoleAccessLevel obj)
        {
            var access = _menuService.GetMenuById(obj.MenuId, obj.RoleId);
            var index = getLevelIndex(obj.AccessLevel);
            var charlist = access != null ? access.AccessLevel.ToCharArray() : "#####".ToCharArray();
            if (charlist[index].ToString() == obj.AccessLevel)
            {
                charlist[index] = '#';
            }
            else
            {
                charlist[index] = Convert.ToChar(obj.AccessLevel);
            }
            var newAccessLevel = charlist[0].ToString() + charlist[1].ToString() + charlist[2].ToString() + charlist[3].ToString() + charlist[4].ToString();

            try
            {
                dataOperation.CommandText("auth_RoleAccessUpdate");
                dataOperation.AddParam("role", obj.RoleId);
                dataOperation.AddParam("menu", obj.MenuId);
                dataOperation.AddParam("accesslevels", newAccessLevel);
                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        private int getLevelIndex(string accesslevel)
        {
            var index = 0;
            switch (accesslevel)
            {
                case "C":
                    index = 0;
                    break;
                case "M":
                    index = 1;
                    break;
                case "D":
                    index = 2;
                    break;
                case "V":
                    index = 3;
                    break;
                case "P":
                    index = 4;
                    break;
                default:
                    break;
            }
            return index;
        }


        public bool AddUser(UserRole obj)
        {
            try
            {
                dataOperation.CommandText("auth_UserRoleInsert");
                dataOperation.AddParam("role", obj.RoleId);
                dataOperation.AddParam("user", obj.UserId);
                dataOperation.AddParam("CreateUser", obj.User);
                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool RemoveUser(UserRole obj)
        {
            try
            {
                dataOperation.CommandText("auth_UserRoleDelete");
                dataOperation.AddParam("user", obj.UserId);
                dataOperation.DoExecute();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }



    }
}
