using Blend.BuisnessEntities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.UploadSale
{
    public interface ISaleUploadService
    {
        bool Log(UploadLog obj);
    }
}
