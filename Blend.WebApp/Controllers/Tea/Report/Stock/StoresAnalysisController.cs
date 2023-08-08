using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Stock;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Configuration;

namespace Blend.WebApp.Controllers.Tea.Report.Stock
{
    public class StoresAnalysisController : BaseApi
    {
        private IStockReportService _stockReportService;
        private ICompanyService _companyService;
        public StoresAnalysisController(IStockReportService stockReportService, ICompanyService companyService)
        {
            _stockReportService = stockReportService;
            _companyService = companyService;
        }

        private object GetStockXmlElement(IList<StoresAnalysisData> data)
        {
            var xmlfromLINQ = new XElement("storesdata",
            from c in data
            select new XElement("item",
                new XElement("BoxNo", c.BoxNo),
                new XElement("CollectionDate", c.CollectionDate),
                new XElement("ReceivedPacks", c.ReceivedPacks),
                new XElement("ClosedDate", c.ClosedDate),
                new XElement("DayCount", c.DayCount),
                new XElement("Packs", c.Packs),
                new XElement("Qty", c.Qty),
                new XElement("Status", c.Status)
                ));
            return xmlfromLINQ;
        }

        private HttpResponseMessage GetNoDataPage()
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        public HttpResponseMessage Post([FromBody]StoresAnalysis obj)
        {
            if (ModelState.IsValid)
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _stockReportService.GetStoresAnalysis(obj);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo();
                    var xmlfromLINQ = GetStockXmlElement(data);
                    var textReader = new StringReader(xmlfromLINQ.ToString());
                    var strReportName = "storeanalysis.rpt";
                    var sysver = ConfigurationManager.AppSettings["sys_ver"];
                    var syscontact = ConfigurationManager.AppSettings["sys_contact"];

                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Stock\\StoresAnalysis\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                    rd.SetParameterValue("fromdate", obj.FromDate.Substring(0, 10));
                    rd.SetParameterValue("todate", obj.ToDate);


                    Stream ms = null;
                    ms = obj.IsExcelFormat ? rd.ExportToStream(ExportFormatType.Excel) : rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    var mediaType = "";
                    mediaType = obj.IsExcelFormat ? "application/vnd.ms-excel" : "application/pdf";
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
