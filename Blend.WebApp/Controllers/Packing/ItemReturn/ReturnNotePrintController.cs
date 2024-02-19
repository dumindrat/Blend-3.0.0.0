using Blend.BuisnessEntities.Packing.ItemReturn;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.ItemReturn;
using Blend.BuisnessService.Packing.PO;
using Blend.WebApp.App.ActionFilters;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Blend.WebApp.Controllers.Packing.PO
{
    public class ReturnNotePrintController : BaseApi
    {
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;
        public IItemRetunService _itemReturnSvc;
        private IItemReceiveService _itemReceiveSvc;

        public ReturnNotePrintController(IPurchaseOrderService poSvc,
            ICompanyService companyService,
            ISupplierService supplierService,
            IItemRetunService itemReturnSvc,
            IItemReceiveService itemReceiveSvc)
        {
            _poSvc = poSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _itemReturnSvc = itemReturnSvc;
            _itemReceiveSvc = itemReceiveSvc;
        }
        // POST api/blend
        [DataOperationAuthorization(operationType = "Print")]
        public HttpResponseMessage Post([FromBody]ItemReturnNoteDto obj)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            var strReportName = "rn.rpt";

            try
            {
                SetUser(obj);
                var stream = new ItemReturnPrint(_poSvc, _companyService, _supplierService,_itemReturnSvc,_itemReceiveSvc).CreateStream(obj);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return result;

        }

    }
}
