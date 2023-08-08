using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Download
{
    public interface IDownloadFileService
    {
        string SaveAsycudaFile(int blendId);
        string SaveSLAsTextFile(int blendId);
        string SaveBLAsTextFile(int blendId);

    }
}
