using Blend.BuisnessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.User
{
    public interface IUserService
    {
        UserEntity Authenticate(UserEntity user);
        bool Authorize(string userName, string operation, string menu);
        IList<UserEntity> GetUsers(string serchStr);
        IList<UserEntity> GetAddedUsersInRole(string serchStr, int role);
        bool ChangePassword(string userId, string passWord);
        UserEntity GetUserById(string userId);
    }
}
