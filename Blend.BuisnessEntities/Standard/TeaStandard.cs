using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Standard
{
    public class TeaStandard : BaseEntity
    {
        public int StandardId { get; set; }
        public string StandardName { get; set; }
    }

    public class TeaStandardForContract 
    {
        public int ContractId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; }
        public string OrderQty { get; set; }
        public int CompanyId { get; set; }
    }
}
