using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.LineSummary;
using Blend.BuisnessService.Adjustent;
using Blend.BuisnessService.BlendCreation;
using Blend.BuisnessService.LineSummary;
using Blend.BuisnessEntities.Purchase;

namespace Blend.WebApp.Controllers.Tea.LineSummary
{
    public class LineSummaryController : BaseApi
    {
        private ILineSummaryService _lineSummarySvc;
        private IBlendCreationService _blendCreateSvc;
        private IAdjustmentService _adjustSvc;

        public LineSummaryController(ILineSummaryService lineSummaryService, IBlendCreationService blendCreateSvc, IAdjustmentService adjustSvc)
        {
            _lineSummarySvc = lineSummaryService;
            _blendCreateSvc = blendCreateSvc;
            _adjustSvc = adjustSvc;
        }

        // GET api/blend/5
        [HttpGet]
        [ActionName("GetLineDetails")]
        public IEnumerable<LineSummaryDto> GetLineDetails(string id)
        {
            IEnumerable<LineSummaryDto> lines = new List<LineSummaryDto>();
            try
            {
                var boxno = id.Replace('~', '/');
                lines = _lineSummarySvc.GetLineDetails(boxno);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return lines;
        }

        [HttpGet]
        [ActionName("GetBlendAllocationsByBoxNo")]
        public IEnumerable<BlendItemAllocation> GetBlendAllocationsByBoxNo(string id)
        {
            IEnumerable<BlendItemAllocation> itemlist = new List<BlendItemAllocation>();
            try
            {
                var boxno = id.Replace('~', '/');
                itemlist = _blendCreateSvc.GetBlendAllocationsByBoxNo(boxno);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return itemlist;
        }

        [HttpGet]
        [ActionName("GetAdjustedLine")]
        public IEnumerable<AdjutedLineDto> GetAdjustedLine(string id)
        {
            IEnumerable<AdjutedLineDto> data = new List<AdjutedLineDto>();
            try
            {
                var boxno = id.Replace('~', '/');
                data = _adjustSvc.GetAdjustedLineByBoxNo(boxno);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return data;
        }

        [HttpGet]
        [ActionName("GetTransactionHistory")]
        public IEnumerable<LineTransHistory> GetTransactionHistory(string id)
        {
            IEnumerable<LineTransHistory> data = new List<LineTransHistory>();
            try
            {
                data = _lineSummarySvc.GetLineTransHistory(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return data;
        }

        [HttpGet]
        [ActionName("GetFilterData")]
        public IEnumerable<LineSummaryDto> GetFilterData(string fromSaleDate, string toSaleDate,
            string invoice, string lotNo, string gardenMark, string brokerCode, string standard)//[FromBody]LineAdvanceFilterDto obj
        {
            IEnumerable<LineSummaryDto> lines = new List<LineSummaryDto>();

            try
            {
                var obj = new LineAdvanceFilterDto
                {
                    FromSaleDate = fromSaleDate,
                    ToSaleDate = toSaleDate,
                    Invoice = invoice == null ? "" : invoice,
                    LotNo = lotNo == null ? "" : lotNo,
                    GardenMark = gardenMark == null ? "" : gardenMark,
                    Standard = standard == null ? "" : standard,
                    BrokerCode = brokerCode == null ? "" : brokerCode
                };

                lines = _lineSummarySvc.GetLineDetailsByAdvanceFilter(obj);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return lines;
        }
    }
}
