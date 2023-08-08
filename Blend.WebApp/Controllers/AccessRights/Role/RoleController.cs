using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities;
using Blend.BuisnessEntities.AccessRights.Role;
using Blend.BuisnessService.AccessRights.Roles;
using Blend.BuisnessService.User;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.AccessRights.Role
{
    //[AuthorizationRequired]
    public class RoleController : BaseApi
    {
        private IRoleService _roleService;
        private IUserService _userService;

        public RoleController(IRoleService roleService, IUserService userService)
        {
            _roleService = roleService;
            _userService = userService;

        }
        public string Get(int id)
        {
            return "value";
        }


        [HttpGet]
        [ActionName("GetAllRoles")]
        public IEnumerable<RoleEntity> GetAllRoles(string id)
        {
            IList<RoleEntity> roles = new List<RoleEntity>();
            if (ModelState.IsValid)
            {
                try
                {
                    roles = _roleService.GetRoles(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");

                }
            }
            return roles;
        }

        [HttpGet]
        [ActionName("GetRoleAccesssLevels")]
        public IEnumerable<RoleAccessLevel> GetRoleAccesssLevels(int id)
        {
            IList<RoleAccessLevel> roles = new List<RoleAccessLevel>();
            if (ModelState.IsValid)
            {
                try
                {
                    roles = _roleService.GetRoleAccessLevels(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return roles;
        }

        [HttpGet]
        [ActionName("GetUsers")]
        public IEnumerable<UserEntity> GetUsers(string id)
        {
            IList<UserEntity> users = new List<UserEntity>();
            if (ModelState.IsValid)
            {
                try
                {
                    users = _userService.GetUsers(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return users;
        }

        [HttpGet]
        [ActionName("GetAddedUsers")]
        //[Route("api/Role/{id}/{role}")]
        public IEnumerable<UserEntity> GetAddedUsers(string id, int role)
        {
            IList<UserEntity> users = new List<UserEntity>();
            if (ModelState.IsValid)
            {
                try
                {
                    users = _userService.GetAddedUsersInRole(id, role);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return users;
        }

        // POST api/role
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]RoleEntity obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    _roleService.CreateRole(obj);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/role/5
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]RoleEntity obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    _roleService.UpdateRole(obj);
                    logger.LogWrite("Role updated success: " + obj.RoleId + "-" + obj.RoleDescription, "Info");
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // DELETE api/role/5
        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _roleService.DeleteRole(new RoleEntity { RoleId = id });
                logger.LogWrite("Role deleted succcess: " + id, "Info");

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {

                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
    }
}
