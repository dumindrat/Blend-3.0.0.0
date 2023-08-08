using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Report
{
    public class BlendSheetRptDto
    {
        public int BlendId { get; set; }
        public bool IsWithValues { get; set; }
        public bool AfterClosed { get; set; }
        public bool IsDownload { get; set; }
        public string FileType { get; set; }

    }
}
