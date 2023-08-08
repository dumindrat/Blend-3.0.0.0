using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessService.AccessRights.Roles;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.AccessRights.Role
{
    public class RoleAccessLevelController : BaseApi
    {
        private IRoleService _roleService;

        public RoleAccessLevelController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Put([FromBody]RoleAccessLevel obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _roleService.UpdateRoleAccessLevels(obj);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);

        }
    }
}
