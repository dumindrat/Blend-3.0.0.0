using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.CompanyRegistration
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public int SubscriptionId { get; set; }
        public string CompanyName { get; set; }
        public string TCEPR { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string StoresAddress1 { get; set; }
        public string StoresAddress2 { get; set; }
        public string StoreTelephone { get; set; }
        public string SubscriptionEndDate { get; set; }

    }

    public class StoresDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string ContactPerson { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
    }
}
