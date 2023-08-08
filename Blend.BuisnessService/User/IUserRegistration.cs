using Blend.BuisnessEntities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.User
{
    public interface IUserRegistration
    {
        void CreateUser(UserRegistration registration);
    }
}
