using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.Data.Authentication.User
{
    public interface IUserSvc
    {
        int Authenticate(string userName, string password);
    }
}
