using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Menu
{
    public class MenuEntity
    {
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string href { get; set; }
        public string AccessLevel { get; set; }
        public string Category { get; set; }
        public int ModuleId { get; set; }
        public int MainModule { get; set; }
        public bool IsQuickLink { get; set; }
    }
}
