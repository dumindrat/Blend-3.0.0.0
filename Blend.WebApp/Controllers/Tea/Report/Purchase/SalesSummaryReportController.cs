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
using Blend.BuisnessEntities.CompanyRegistration;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Purchase;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Blend.WebApp.Controllers.Tea.Report.Purchase
{
    public class SalesSummaryReportController : BaseApi
    {
        private IPurchaseReportService _purchaseRptService;
        private ICompanyService _companyService;
        private string sysver, syscontact;
        //private CompanyDto companyInfo;

        public SalesSummaryReportController(IPurchaseReportService purchaseRptService, ICompanyService companyService)
        {
            _purchaseRptService = purchaseRptService;
            _companyService = companyService;
            //companyInfo = _companyService.GetCompanyInfo();

            sysver = ConfigurationManager.AppSettings["sys_ver"];
            syscontact = ConfigurationManager.AppSettings["sys_contact"];
        }

        private HttpResponseMessage GetNoDataPage()
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }



        public HttpResponseMessage Post([FromBody]PurchaseSheetRptDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _purchaseRptService.GetSaleSummary(obj.SaleId);
                    var saleDate = data.Select(c => c.SaleDate).Distinct().First();
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetSalesSummaryXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "salessummarynew.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\SalesSummary\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("saledate", saleDate);

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

        private XElement GetSalesSummaryXmlElement(IList<SaleSummaryDto> data)
        {
            var xmlfromLINQ = new XElement("salesummary",
              from c in data
              select new XElement("item",
                  new XElement("NoOfLots", c.NoOfLots),
                  new XElement("BrokerId", c.BrokerId),
                  new XElement("BrokerName", c.BrokerName),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("Weight", c.Weight),
                  new XElement("Value", c.Value),
                  new XElement("SaleType", c.SaleType),
                  new XElement("NinetyPercVal", c.NinetyPercVal),
                  new XElement("TenPercVal", c.TenPercVal),
                  new XElement("HundredPercVal", c.HundredPercVal)

                  ));
            return xmlfromLINQ;
        }
    }
}
