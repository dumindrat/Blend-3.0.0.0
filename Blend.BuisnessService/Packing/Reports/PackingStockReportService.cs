using Blend.BuisnessEntities.Packing.Reports;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.Reports
{
    public class PackingStockReportService : IPackingStockReportService
    {
        public IList<BuisnessEntities.Packing.Reports.PackingStockDto> GetPackingStock(int companyId)
        {
            var mapper = new DataMapper<PackingStockDto>("GetPackingStock", new Hashtable() { { "CompanyId", companyId } });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
