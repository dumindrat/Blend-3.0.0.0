using Blend.BuisnessEntities.Menu;
using Blend.BuisnessService.Menu;
using Blend.Data;
using Blend.WebApp.LogHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Menu
{
    public class MenuController : ApiController
    {
        LogWriter logger = new LogWriter();
        private IOperation<MenuEntity> dataManager;
        private IMenuService _menuServce;

        public MenuController(IMenuService menuService)
        {
            _menuServce = menuService;
        }
        // GET api/menu
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/menu/5
       
        [HttpGet]
        [ActionName("GetMenuByUserAndMainModule")]
        public IEnumerable<MenuEntity> Get(string id, int mainmodule)
        {
            IEnumerable<MenuEntity> menuList = new List<MenuEntity>();
            if (ModelState.IsValid)
            {
                try
                {
                    menuList = _menuServce.GetMenuByUserNameAndMainModule(id, mainmodule);
                }
                catch (Exception ex)
                {
                    logger.LogWrite("Error in Menu loading. " + ex.Message, "menu");

                }
            }
            return menuList;
        }
    }
}
