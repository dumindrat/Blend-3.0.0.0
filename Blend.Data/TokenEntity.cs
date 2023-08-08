using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data
{
    public class TokenEntity
    {
        public string LoginId { get; set; }
        public string AuthToken { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
