using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.TransDocument
{
    public class DocumentNo : BaseEntity
    {
        public int DocType { get; set; }
        public int DocNo { get; set; }
        public string Prefix { get; set; }
        public string DocFormat { get; set; }
        public string FormattedDocNo { get; set; }
        public string Description { get; set; }
        public int ModuleId { get; set; }
    }
}
