using Blend.BuisnessEntities.User;
using Blend.BuisnessService.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.User
{
    public class UserRegistrationService : BaseManagement<UserRegistration>, IUserRegistration
    {
        public void CreateUser(BuisnessEntities.User.UserRegistration registration)
        {
            var password = EncryptData.EncryptText(registration.Password);

            try
            {
                dataOperation.CommandText("auth_UserInsert");
                dataOperation.AddParam("LoginId", registration.UserName);
                dataOperation.AddParam("PsWord", password);
                dataOperation.AddParam("EmailAddress", registration.Email);
                dataOperation.AddParam("FullName", registration.Name);
                dataOperation.DoExecute();
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
