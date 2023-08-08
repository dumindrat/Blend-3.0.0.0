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
    public class BrokerPurchaseController : BaseApi
    {
        private IPurchaseReportService _purchaseRptService;
        private ICompanyService _companyService;
        private string sysver, syscontact;
        private CompanyDto companyInfo;

        public BrokerPurchaseController(IPurchaseReportService purchaseRptService, ICompanyService companyService)
        {
            _purchaseRptService = purchaseRptService;
            _companyService = companyService;
            companyInfo = _companyService.GetCompanyInfo();

            sysver = ConfigurationManager.AppSettings["sys_ver"];
            syscontact = ConfigurationManager.AppSettings["sys_contact"];
        }
        private HttpResponseMessage GetNoDataPage()
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        private XElement GetPurchaseSheetXmlElement(IList<PurchaseSheetDto> data)
        {
            var xmlfromLINQ = new XElement("purchasesheet",
              from c in data
              select new XElement("item",
                  new XElement("BoxNo", c.BoxNo),
                  new XElement("TinNo", c.TinNo),
                  new XElement("LotNo", c.LotNo),
                  new XElement("Invoice", c.Invoice),
                  new XElement("Grade", c.Grade),
                  new XElement("Standard", c.Standard),
                  new XElement("BrokerName", c.BrokerName),
                  new XElement("GardenMark", c.GardenMark),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("SaleType", c.SaleType),
                  new XElement("SaleCode", c.SaleCode),
                  new XElement("NetWeight", c.NetWeight),
                  new XElement("Packs", c.Packs),
                  new XElement("Weight", c.Weight),
                  new XElement("Rate", c.Rate),
                  new XElement("Value", c.Value),
                  new XElement("IsRepeated", c.IsRepeated),
                  new XElement("VirtualValue", c.VirtualValue),
                  new XElement("VirtualWeight", c.VirtualWeight)
                  ));
            return xmlfromLINQ;
        }

        // POST api/brokerpurchase
        public HttpResponseMessage Post([FromBody]BrokerPurchaeRptDto obj)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            if (ModelState.IsValid)
            {
                try
                {
                    var data = _purchaseRptService.GetPurchaseSheetByBroker(obj);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo();
                    var xmlfromLINQ = GetPurchaseSheetXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "brokerpurchases.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\PurchaseSheet\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("fromSale", obj.FromSaleDate);
                    rd.SetParameterValue("toSale", obj.ToSaleDate);
                    rd.SetParameterValue("broker", obj.BrokerName);
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
            }
            return result;

        }
    }
}
