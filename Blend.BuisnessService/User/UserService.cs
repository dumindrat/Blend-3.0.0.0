using Blend.BuisnessEntities;
using Blend.BuisnessEntities.Menu;
using Blend.BuisnessService.Token;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.User
{
    public class UserService : BaseManagement<UserEntity>, IUserService
    {
        public UserEntity Authenticate(UserEntity user)
        {
            try
            {
                var mapper = new DataMapper<UserEntity>("GetUserByUserName", user);

                var result = mapper.Object();
                if (result != null)
                {
                    var password = EncryptData.DecryptText(result.Password);
                    if (password == user.Password)
                    {
                        result.IsAuthenticated = true;
                        logger.LogWrite("Authenticated User success :" + result.UserName);

                        return result;
                    }
                }
                logger.LogWrite("User Authenticate fails. No User found for " + user.UserName);
                return user;
            }
            catch (Exception ex)
            {
                logger.LogWrite("User Authenticate fails. Error " + ex.Message);
                return user;
            }

        }



        public bool Authorize(string userName, string operation, string menu)
        {
            try
            {
                var mapper = new DataMapper<MenuEntity>("GetUserAccessLevelByUserName", new MenuEntity { UserName = userName, MenuName = menu });

                var result = mapper.Object();
                if (result != null && !String.IsNullOrEmpty(result.AccessLevel))
                {
                    if (result.AccessLevel.Contains(OperationChar(operation)))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogWrite("User Authorize fails. Error " + ex.Message);
                return false;
            }

        }

        private string OperationChar(string operation)
        {
            switch (operation)
            {
                case "Create":
                    return "C";
                case "Update":
                    return "M";
                case "Delete":
                    return "D";
                case "View":
                    return "V";
                case "Print":
                    return "P";

                default:
                    return "#";
            }
        }


        public IList<UserEntity> GetUsers(string serchStr)
        {
            try
            {
                var searchBy = serchStr == "all" ? "" : serchStr;
                var mapper = new DataMapper<UserEntity>("GetUsersNotInRole", new Hashtable() { { "UserName", searchBy } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public IList<UserEntity> GetAddedUsersInRole(string serchStr, int role)
        {
            try
            {
                var searchBy = serchStr == "all" ? "" : serchStr;
                var mapper = new DataMapper<UserEntity>("GetUsersInRole", new Hashtable() { { "UserName", searchBy }, { "RoleId", role } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public bool ChangePassword(string userId, string passWord)
        {

            try
            {
                dataOperation.CommandText("auth_UserUpdatePassword");
                dataOperation.AddParam("@UserNme", userId);
                dataOperation.AddParam("@Password", passWord);
                dataOperation.DoExecute();
                return true;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public UserEntity GetUserById(string userId)
        {
            var mapper = new DataMapper<UserEntity>("GetUserByUserName", new UserEntity { UserName = userId });

            var result = mapper.Object();
            return result;

        }
    }
}
