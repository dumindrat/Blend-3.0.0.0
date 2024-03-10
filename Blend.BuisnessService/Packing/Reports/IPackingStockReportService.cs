using Blend.BuisnessEntities.Packing.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.Reports
{
    public interface IPackingStockReportService
    {
        IList<PackingStockDto> GetPackingStock(int companyId);
    }
}
