using Blend.BuisnessEntities.Packing.ItemIssue;
using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.BlendCreation;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemIssue;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.PO;
using Blend.BuisnessService.Reports.Blend;
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
    public class IssueNotePrintController : BaseApi
    {
        private IBlendReportService _blSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;
        private IItemIssueService _itemmIssueSvc;
        public IssueNotePrintController(IBlendReportService blSvc, ICompanyService companyService,
            ISupplierService supplierService, IItemIssueService itemIssueSvc)
        {
            _blSvc = blSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _itemmIssueSvc = itemIssueSvc;
        }
        // POST api/blend
        [DataOperationAuthorization(operationType = "Print")]
        public HttpResponseMessage Post([FromBody]ItemIssueNoteDto obj)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            var strReportName = "ir.rpt";

            try
            {
                SetUser(obj);
                var stream = new INPrint(_itemmIssueSvc, _companyService, _supplierService, _blSvc).CreateStream(obj);
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
