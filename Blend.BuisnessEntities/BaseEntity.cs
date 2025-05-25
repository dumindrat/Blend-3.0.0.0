using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities
{
    public abstract class BaseEntity
    {
        public int CompanyId { get; set; }
        public string AddedDate { get; set; }
        public string ModifiedDate { get; set; }
        public byte[] RowVersion { get; set; }
        public string OperationCommand { get; set; }
        public string User { get; set; }
        public string ModifiedUser { get; set; }
        public string CloseStatus { get; set; }
    }
}
