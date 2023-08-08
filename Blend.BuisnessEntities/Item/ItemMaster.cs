using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Item
{
    public class ItemMaster : BaseEntity
    {
        public int Id { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int UomId { get; set; }
        public string UomName { get; set; }
        public string ShortDescription { get; set; }

    }

    public class ItemProperty : BaseEntity
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemPropertyName { get; set; }

    } 
}
