using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Download.Asycuda
{
    public class Helper
    {
        private string m_exePath = Path.GetDirectoryName(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
        public void CreateFile(string blendNo)
        {
            var file = m_exePath + "\\" + "Logs" + "\\" + blendNo + ".xls";

        }
    }
}
