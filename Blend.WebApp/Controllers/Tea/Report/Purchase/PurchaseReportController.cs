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
    public class PurchaseReportController : BaseApi
    {
        private IPurchaseReportService _purchaseRptService;
        private ICompanyService _companyService;
        private string sysver, syscontact;
        //private CompanyDto companyInfo;

        public PurchaseReportController(IPurchaseReportService purchaseRptService, ICompanyService companyService)
        {
            _purchaseRptService = purchaseRptService;
            _companyService = companyService;
            //companyInfo = _companyService.GetCompanyInfo();

            sysver = ConfigurationManager.AppSettings["sys_ver"];
            syscontact = ConfigurationManager.AppSettings["sys_contact"];
        }
        // GET api/purchasereport
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/purchasereport/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [ActionName("GetSalesSummary")]
        public HttpResponseMessage GetSalesSummary(int id)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var data = _purchaseRptService.GetSaleSummary(id);
                if (data.Count == 0)
                    return GetNoDataPage();
                var xmlfromLINQ = GetSalesSummaryXmlElement(data);

                var textReader = new StringReader(xmlfromLINQ.ToString());
                var companyInfo = _companyService.GetCompanyInfo(GetCompanyId());

                var strReportName = "salessummarynew.rpt";
                var rd = new ReportDocument();
                DataSet ds_fldDef = new DataSet();

                string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\SalesSummary\\" + strReportName;
                rd.Load(strPath);
                ds_fldDef.ReadXml(textReader);
                rd.SetDataSource(ds_fldDef);
                rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                result.Content = new StreamContent(ms);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                //Log
            }
            return result;

        }

        //[HttpGet]
        //[ActionName("GetPurchaseSheet")]
        //public HttpResponseMessage GetPurchaseSheet(int id)
        //{
        //    var result = Request.CreateResponse(HttpStatusCode.OK);
        //    try
        //    {

        //        var data = _purchaseRptService.GetPurchaseSheet(id);

        //        if (data.Count == 0)
        //            return GetNoDataPage();
        //        var xmlfromLINQ = GetPurchaseSheetXmlElement(data);

        //        var textReader = new StringReader(xmlfromLINQ.ToString());

        //        var strReportName = "purchasesheet.rpt";
        //        var rd = new ReportDocument();
        //        DataSet ds_fldDef = new DataSet();

        //        string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\PurchaseSheet\\" + strReportName;
        //        rd.Load(strPath);
        //        ds_fldDef.ReadXml(textReader);
        //        rd.SetDataSource(ds_fldDef);
        //        rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);

        //        Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
        //        result.Content = new StreamContent(ms);
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
        //        result.Content.Headers.ContentDisposition.FileName = strReportName;
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        //Log
        //    }
        //    return result;

        //}

        [HttpGet]
        [ActionName("GetCanisterSheet")]
        public HttpResponseMessage GetCanisterSheet(int id)
        {
            var canisterType = ConfigurationManager.AppSettings["CanisterType"];
            var result = Request.CreateResponse(HttpStatusCode.OK);
            try
            {

                var data = _purchaseRptService.GetCanisterSheet(id);

                if (data.Count == 0)
                    return GetNoDataPage();
                var xmlfromLINQ = GetCanisterSheetXmlElement(data);

                var textReader = new StringReader(xmlfromLINQ.ToString());

                var strReportName = canisterType == "1" ? "canistersheet.rpt" : "canistersheet_rowtype.rpt";
                var rd = new ReportDocument();
                DataSet ds_fldDef = new DataSet();

                string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\Canisters\\" + strReportName;
                rd.Load(strPath);
                ds_fldDef.ReadXml(textReader);
                rd.SetDataSource(ds_fldDef);
                //rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);

                Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                result.Content = new StreamContent(ms);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                //Log
            }
            return result;

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
                  new XElement("TeaCertificates", c.TeaCertificates),
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

        private XElement GetCanisterSheetXmlElement(IList<CanisterSheetDto> data)
        {
            var xmlfromLINQ = new XElement("canistersheet",
              from c in data
              select new XElement("item",
                  new XElement("TinNo", c.TinNo),
                  new XElement("BoxNo", c.BoxNo),
                  new XElement("LotNo", c.LotNo),
                  new XElement("Invoice", c.Invoice),
                  new XElement("Grade", c.Grade),
                  new XElement("Standard", c.Standard),
                  new XElement("BrokerName", c.BrokerName),
                  new XElement("GardenMark", c.GardenMark),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("SaleType", c.SaleType),
                  new XElement("Rate", c.Rate),
                  new XElement("LineQty", c.LineQty),
                  new XElement("RepeatedLineQty", c.RepeatedLineQty),
                  new XElement("Qty", c.Qty)
                  ));
            return xmlfromLINQ;
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
                    var data = _purchaseRptService.GetPurchaseSheet(obj.SaleId);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetPurchaseSheetXmlElement(data);

                    var recordCount = data.Where(c => c.IsRepeated == false).Count();

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = obj.IsWithValues == true ? "purchasesheet.rpt" : "purchasesheetstores.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\PurchaseSheet\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("recordcount", recordCount);

                    Stream ms = null;
                    //ms = obj.IsExcelFormat ? rd.ExportToStream(ExportFormatType.Excel) : rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    var mediaType = "";
                    mediaType =  "application/pdf";
                    if (!obj.IsDownload)
                    {
                        ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                        mediaType = "application/pdf";
                    }
                    if (obj.IsDownload && obj.FileType == "xls")
                    {
                        ms = rd.ExportToStream(ExportFormatType.Excel);
                        mediaType = "application/vnd.ms-excel";
                    }
                    if (obj.IsDownload && obj.FileType == "doc")
                    {
                        ms = rd.ExportToStream(ExportFormatType.WordForWindows);
                        mediaType = "application/msword";
                    }
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
