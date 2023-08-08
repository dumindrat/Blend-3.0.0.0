using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Adjustment;

namespace Blend.BuisnessEntities.Report
{
    public class NotePrint
    {
        public int BrokerId { get; set; }
        public List<PrintNoteDto> BoxNoList { get; set; }
        public string ForWhom { get; set; }
        public string AuthorizedPerson { get; set; }
    }

    public class ClaimNotePrint : NotePrint
    {
        public string Reason { get; set; }
    }


}
