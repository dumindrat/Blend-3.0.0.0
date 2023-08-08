using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.User;
using Blend.BuisnessService.AccessRights.Roles;

namespace Blend.WebApp.Controllers.AccessRights.Role
{
    public class UserRoleController : BaseApi
    {
        private IRoleService _roleService;

        public UserRoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // POST api/userrole
        public HttpResponseMessage Post([FromBody]UserRole obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    _roleService.AddUser(obj);
                    logger.LogWrite("User " + obj.UserId + "added to the Role " + obj.RoleId, "Info");
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }



        // DELETE api/userrole/5
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _roleService.RemoveUser(new UserRole { UserId = id });
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
    }
}
