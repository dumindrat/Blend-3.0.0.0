using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Purchase;
using Blend.WebApp.App.ActionFilters;
using Blend.BuisnessService.UploadSale;
using Newtonsoft.Json;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class StraightLineUploadController : BaseApi
    {
        private IPurchaseService _purchaseSvc;
        private ISaleUploadService _saleUploadService;
        public StraightLineUploadController(IPurchaseService purchaseService, ISaleUploadService saleUploadService)
        {
            _purchaseSvc = purchaseService;
            _saleUploadService = saleUploadService;
        }

        [DataOperationAuthorization(operationType = "Update")]
        public UploadStatus Post([FromBody]SaleUploadDto saleUpload)
        {
            SetUser(saleUpload);
            var lines = new List<StraightLine>();
            var uplaodStatus = new UploadStatus() { UploadData = new List<SraightLineUploadStatus>() };
            int successCount = 0;
            int errorCount = 0;
            var fileName = saleUpload.FileName;
            var saleId = saleUpload.SaleId;
            foreach (var starightLine in saleUpload.StraightLines)
            {
                if (SetUser(starightLine))
                {
                    try
                    {
                        _purchaseSvc.AddStraightLine(starightLine);
                        uplaodStatus.UploadData.Add(new SraightLineUploadStatus { LotNo = starightLine.LotNo, Status = "SUCCESS" });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWrite(ex.Message, "Error");
                        uplaodStatus.UploadData.Add(new SraightLineUploadStatus { LotNo = starightLine.LotNo, Status = "FAIL", Message = ex.Message });
                        uplaodStatus.HttpCode = 400;
                        errorCount++;
                    }
                }
            }
            uplaodStatus.SuccessCount = successCount;
            uplaodStatus.ErrorCount = errorCount;
            uplaodStatus.HttpCode = 200;
            _saleUploadService.Log(new UploadLog
            {
                SaleId = saleUpload.SaleId,
                FileName = saleUpload.FileName,
                ErrorRecords = errorCount,
                BrokerId = saleUpload.BrokerId,
                SaleCode = saleUpload.SaleCode,
                SaleType = saleUpload.SaleType,
                TotalRecords = saleUpload.StraightLines.Count,
                UploadData = JsonConvert.SerializeObject(GetUploadStraightLines(saleUpload.StraightLines)),
                User = saleUpload.User
            });
            return uplaodStatus;
        }

        private List<UploadStraightLine> GetUploadStraightLines(List<StraightLine> lines)
        {
            var listLInes = new List<UploadStraightLine>();
            foreach (var line in lines)
            {
                listLInes.Add(new UploadStraightLine
                {
                    LotNo = line.LotNo,
                    InvNo = line.Invoice,
                    Mark = line.GardenMark,
                    Grade = line.Grade,
                    Net = line.Net1.ToString(),
                    Bag = line.Pack1.ToString(),
                    Qty = line.Weight.ToString(),
                    Price = line.Value.ToString(),
                    ReducedQty = line.ReducedQty.ToString()
                });
            }
            return listLInes;
        }
    }
}
