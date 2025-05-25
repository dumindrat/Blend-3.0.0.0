using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
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
    public class POPrintController : BaseApi
    {
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;

        public POPrintController(IPurchaseOrderService poSvc, ICompanyService companyService, ISupplierService supplierService)
        {
            _poSvc = poSvc;
            _companyService = companyService;
            _supplierService = supplierService;
        }
        // POST api/blend
        [DataOperationAuthorization(operationType = "Print")]
        public HttpResponseMessage Post([FromBody]PurchaseOrderDto po)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            var strReportName = "po.rpt";

            try
            {
                SetUser(po);
                var stream = new POPrint(_poSvc,_companyService,_supplierService).CreateStream(po);
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
