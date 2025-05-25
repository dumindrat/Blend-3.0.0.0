using Blend.BuisnessEntities.CompanyRegistration;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Purchase;
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
using System.Xml.Linq;
using Blend.BuisnessService.StoresInstruction;
using Blend.BuisnessEntities.StoresInstruction;
using Blend.BuisnessService.BlendCreation;

namespace Blend.WebApp.Controllers.Tea.Report.Purchase
{
    public class StoresInstructionReportController : BaseApi
    {
        private IStoresInstructionService _iStoresInstructionService;
        private ICompanyService _companyService;
        private string sysver, syscontact;
        private IBlendCreationService _blendSvc;
        public StoresInstructionReportController(IStoresInstructionService iStoresInstructionService,
            ICompanyService companyService, IBlendCreationService blendSvc)
        {
            _iStoresInstructionService = iStoresInstructionService;
            _companyService = companyService;
            _blendSvc = blendSvc;
            sysver = ConfigurationManager.AppSettings["sys_ver"];
            syscontact = ConfigurationManager.AppSettings["sys_contact"];
        }


        private XElement GetReportXmlElement(IEnumerable<StoresInstructions> data)
        {
            var xmlfromLINQ = new XElement("storesins",
              from c in data
              select new XElement("item",
                  new XElement("Id", c.Id),
                  new XElement("StoresInstruction", c.StoresInstruction)
                  ));
            return xmlfromLINQ;
        }


        private HttpResponseMessage GetNoDataPage()
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        public HttpResponseMessage Post([FromBody]StoresInstructions obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _iStoresInstructionService.GetStoresInstructionsByBlend(obj.BlendId).ToList();
                    var blendInfo = _blendSvc.GetBlendInfoById(obj.BlendId);
                    
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetReportXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "storesinstructions.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\BlendSheet\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("blendno", blendInfo.BlendNo);
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                    Stream ms = null;
                    ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);

                    var mediaType = "";
                    mediaType = "application/pdf";
                    result.Content = new StreamContent(ms);
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                    result.Content.Headers.ContentDisposition.FileName = strReportName;
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
                return result;
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
