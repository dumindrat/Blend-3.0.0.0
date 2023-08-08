using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.AuctionBrokers
{
    public class TeaBroker : BaseEntity
    {
        public int BrokerId { get; set; }
        public string BrokerCode { get; set; }
        public string BrokerName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string ContactPerson { get; set; }
    }
}
