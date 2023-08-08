using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.LineSummary;

namespace Blend.BuisnessService.LineSummary
{
    public class LineSummaryService : BaseManagement<LineSummaryDto>, ILineSummaryService
    {
        public IEnumerable<LineSummaryDto> GetLineDetails(string boxNo)
        {
            try
            {
                var mapper = new DataMapper<LineSummaryDto>("GetLineSummaryDet", new Hashtable() { { "BoxNo", boxNo } });
                var results = mapper.ObjectList();
                return results;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetLineDetails fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public IEnumerable<LineAllocationDto> GetLineAllocations(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LineAdjustmentDto> GetLineAdjustments(int id)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<LineTransHistory> GetLineTransHistory(int id)
        {
            try
            {
                var mapper = new DataMapper<LineTransHistory>("GetLineHistoryDet", new Hashtable() { { "Id", id } });
                var results = mapper.ObjectList();
                return results;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetLineDetails fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }
    }
}
