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


        public IEnumerable<LineSummaryDto> GetLineDetailsByAdvanceFilter(BuisnessEntities.Purchase.LineAdvanceFilterDto obj)
        {
            try
            {
                var mapper = new DataMapper<LineSummaryDto>("GetLineSummaryfilter", new LineFilterParam()
                {
                    BrokerCode = obj.BrokerCode,
                    FromSaleDate = obj.FromSaleDate,
                    ToSaleDate = obj.ToSaleDate,
                    GardenMark = obj.GardenMark,
                    Invoice = obj.Invoice,
                    LotNo = obj.LotNo,
                    Standard = obj.Standard
                });
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

    public class LineFilterParam
    {
        public string FromSaleDate { get; set; }
        public string ToSaleDate { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public string BrokerCode { get; set; }
    }
}
