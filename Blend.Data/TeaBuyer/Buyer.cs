using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data.TeaBuyer
{
    public class Buyer : BaseEntity
    {
        public long BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

    }
   
}
