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

namespace Blend.WebApp.Controllers.Tea.Report.Purchase
{
    public class PurchaseReportAdvanceController : BaseApi
    {
        private IPurchaseReportService _purchaseRptService;
        private ICompanyService _companyService;
        private string sysver, syscontact;

        public PurchaseReportAdvanceController(IPurchaseReportService purchaseRptService, ICompanyService companyService)
        {
            _purchaseRptService = purchaseRptService;
            _companyService = companyService;

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

        public HttpResponseMessage Post([FromBody]PurchaseSheetAdvanceRptDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _purchaseRptService.GetPurchaseSheetByAdvanceFilter(obj);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetPurchaseSheetXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    //var strReportName = obj.IsWithValues == true ? "purchasesheet.rpt" : "purchasesheetstores.rpt";
                    var strReportName = "advancepurchasesheet.rpt";

                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\PurchaseSheet\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("fromsaledate", obj.FromSaleDate);
                    rd.SetParameterValue("tosaledate", obj.ToSaleDate);
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
