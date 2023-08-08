using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessService.BlendCreation;
using Blend.BuisnessService.Contract;
using Blend.BuisnessService.Download;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Blend.WebApp.Controllers.BlendCreation
{
    public class BlendController : BaseApi
    {
        private IBlendCreationService _blendSvc;
        private IContractService _contractSvc;
        private IDownloadFileService _downloadFile;
        public BlendController(IBlendCreationService blendSvc, IContractService contractSvc, IDownloadFileService downloadFile)
        {
            _blendSvc = blendSvc;
            _contractSvc = contractSvc;
            _downloadFile = downloadFile;
        }
        // GET api/blend
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/blend/5
        [HttpGet]
        [ActionName("GetBlends")]
        public IEnumerable<BlendInfo> Get(string id)
        {
            IEnumerable<BlendInfo> blends = new List<BlendInfo>();
            try
            {
                id = id.Replace("~", "/");
                blends = _blendSvc.GetBlendList(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return blends;
        }

        [HttpGet]
        [ActionName("GetStraghtLineStock")]
        public IEnumerable<StraightLineStockItem> GetStraghtLineStock(string id)
        {
            IEnumerable<StraightLineStockItem> stocks = new List<StraightLineStockItem>();
            try
            {
                stocks = _blendSvc.GetStraightLIneStock(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return stocks;
        }

        [HttpGet]
        [ActionName("GetBlendStock")]
        public IEnumerable<BlendStockItem> GetBlendStock(string id)
        {
            IEnumerable<BlendStockItem> stocks = new List<BlendStockItem>();
            try
            {
                id = id.Replace("~", "/");
                stocks = _blendSvc.GetBlendStock(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return stocks;
        }

        [HttpGet]
        [ActionName("GetStraghtLineStockByBoxNo")]
        public StraighLineStockDto GetStraghtLineStockByBoxNo(string id)
        {
            var stocks = new StraighLineStockDto();
            try
            {
                id = id.Replace("~", "/");
                stocks = _blendSvc.GetStraightLIneStockByBoxNo(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return stocks;
        }

        [HttpGet]
        [ActionName("GetBlendStockItemByStockId")]
        public StraighLineStockDto GetBlendStockItemByStockId(string id)
        {
            var stocks = new StraighLineStockDto();
            try
            {
                id = id.Replace("~", "/");
                stocks = _blendSvc.GetStraightLIneStockByBoxNo(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return stocks;
        }

        [HttpGet]
        [ActionName("GetQuickSearchItems")]
        public IEnumerable<BlendQuickSearchItem> GetQuickSearchItems(string id)
        {
            IEnumerable<BlendQuickSearchItem> stocks = new List<BlendQuickSearchItem>();
            try
            {
                stocks = _blendSvc.GetBlendQuickSearchItem(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return stocks;
        }

        [HttpGet]
        [ActionName("GetBlendItemSummaryById")]
        public BlendItemSummaryDto GetBlendItemSummaryById(int id)
        {
            var items = new BlendItemSummaryDto();
            items.BlendItemSummaryList = new List<BlendItemSummary>();
            try
            {
                //var listItems = new List<BlendItemSummary>();

                var blendItems = _blendSvc.GetBlendById(id);
                var msItems = blendItems.BlendItem.Where(c => c.SaleType == "MS" || c.SaleType == "OL" && c.ItemType == "L");
                var blendItemList = new List<BlendItemDisplay>();
                foreach (var item in msItems)
                {
                    blendItemList.Add(new BlendItemDisplay { BoxNo = item.BoxNo, SaleDate = item.SaleDate });
                }
                var msItemTotQty = msItems.Sum(c => c.Qty);
                var msItemTotVal = msItems.Sum(c => c.Value);
                var msItemTotCount = msItems.Count();
                //listItems.Add(new BlendItemSummary { LineType = "MS", BlendItems = blendItemList, LineCount = msItemTotCount, Qty = msItemTotQty, Value = msItemTotVal });
                items.BlendItemSummaryList.Add(new BlendItemSummary { LineType = "MS", BlendItems = blendItemList, LineCount = msItemTotCount, Qty = msItemTotQty, Value = msItemTotVal });

                var psItems = blendItems.BlendItem.Where(c => c.SaleType == "PS" && c.ItemType == "L");
                blendItemList = new List<BlendItemDisplay>();
                foreach (var item in psItems)
                {
                    blendItemList.Add(new BlendItemDisplay { BoxNo = item.BoxNo, SaleDate = item.SaleDate });
                }
                var psItemTotQty = psItems.Sum(c => c.Qty);
                var psItemTotVal = psItems.Sum(c => c.Value);
                var psItemTotCount = psItems.Count();
                //listItems.Add(new BlendItemSummary { LineType = "PS", BlendItems = blendItemList, LineCount = psItemTotCount, Qty = psItemTotQty, Value = psItemTotVal });
                items.BlendItemSummaryList.Add(new BlendItemSummary { LineType = "PS", BlendItems = blendItemList, LineCount = psItemTotCount, Qty = psItemTotQty, Value = psItemTotVal });


                var blItems = blendItems.BlendItem.Where(c => c.ItemType != "L");
                blendItemList = new List<BlendItemDisplay>();
                foreach (var item in blItems)
                {
                    blendItemList.Add(new BlendItemDisplay { BoxNo = item.BoxNo, SaleDate = item.SaleDate });
                }
                var blItemTotQty = blItems.Sum(c => c.Qty);
                var blItemTotVal = blItems.Sum(c => c.Value);
                var blItemTotCount = blItems.Count();
                //listItems.Add(new BlendItemSummary { LineType = "BB", BlendItems = blendItemList, LineCount = blItemTotCount, Qty = blItemTotQty, Value = blItemTotVal });
                items.BlendItemSummaryList.Add(new BlendItemSummary { LineType = "BB", BlendItems = blendItemList, LineCount = blItemTotCount, Qty = blItemTotQty, Value = blItemTotVal });
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return items;
        }

        [HttpGet]
        [ActionName("GetBlendAllocationsBylendId")]
        public IEnumerable<BlendItemAllocation> GetBlendAllocationsBylendId(long id)
        {
            IEnumerable<BlendItemAllocation> itemlist = new List<BlendItemAllocation>();
            try
            {
                var bbList = _blendSvc.GetBlendAllocationsById(id, "BB");
                var stdbgList = _blendSvc.GetBlendAllocationsById(id, "STDBG");
                itemlist = bbList.Concat(stdbgList);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return itemlist;
        }

        [HttpGet]
        [ActionName("GetExcelData")]
        public HttpResponseMessage GetAsycudaData(int id)
        {
            return GenerateExcelFile(id);
        }

        [HttpGet]
        [ActionName("GetSLTextData")]
        public HttpResponseMessage GetSLTextData(int id)
        {
            return GenerateSLTextFile(id);
        }

        [HttpGet]
        [ActionName("GetBLTextData")]
        public HttpResponseMessage GetBLTextData(int id)
        {
            return GenerateBLTextFile(id);
        }

        private HttpResponseMessage GenerateExcelFile(int id)
        {
            var filePath = _downloadFile.SaveAsycudaFile(Convert.ToInt16(id));
            var response = GetResponse(filePath);
            return response;
        }

        private HttpResponseMessage GenerateSLTextFile(int id)
        {
            var filePath = _downloadFile.SaveSLAsTextFile(Convert.ToInt16(id));
            var response = GetResponse(filePath);
            return response;
        }

        private HttpResponseMessage GenerateBLTextFile(int id)
        {
            var filePath = _downloadFile.SaveBLAsTextFile(Convert.ToInt16(id));
            var response = GetResponse(filePath);
            return response;
        }

        private HttpResponseMessage GetResponse(string filePath)
        {

            var httpResponseMessage = new HttpResponseMessage();
            try
            {
                var fileName = new FileInfo(filePath).Name;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);

                        httpResponseMessage.Content = new ByteArrayContent(bytes.ToArray());
                        httpResponseMessage.Content.Headers.Add("x-filename", fileName);
                        httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                        httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
                        httpResponseMessage.StatusCode = HttpStatusCode.OK;
                        return httpResponseMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        // POST api/blend
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]BlendInfo blend)
        {
            if (ModelState.IsValid && SetUser(blend))
            {
                try
                {
                    blend.User = System.Threading.Thread.CurrentPrincipal.Identity.Name;
                    _blendSvc.CreateBlend(blend);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]BlendInfo blend)
        {
            if (ModelState.IsValid && SetUser(blend))
            {
                try
                {
                    blend.User = System.Threading.Thread.CurrentPrincipal.Identity.Name;
                    _blendSvc.UpdateBlend(blend);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        // DELETE api/blend/5
        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _blendSvc.DeleteBlend(id);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }
    }
}
