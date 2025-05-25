using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemReceive;
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
    public class IRPrintController : BaseApi
    {
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;
        private IItemReceiveService _itemmReceveSvc;
        public IRPrintController(IPurchaseOrderService poSvc, ICompanyService companyService,
            ISupplierService supplierService,IItemReceiveService itemReceveSvc)
        {
            _poSvc = poSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _itemmReceveSvc = itemReceveSvc;
        }
        // POST api/blend
        [DataOperationAuthorization(operationType = "Print")]
        public HttpResponseMessage Post([FromBody]ItemReceiveNoteDto obj)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            var strReportName = "ir.rpt";

            try
            {
                SetUser(obj);
                var stream = new IRPrint(_itemmReceveSvc,_companyService,_supplierService,_poSvc).CreateStream(obj);
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
