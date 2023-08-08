using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Stock;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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

namespace Blend.WebApp.Controllers.Tea.Report.Stock
{
    public class StockReportController : BaseApi
    {
        private IStockReportService _stockReportService;
        private ICompanyService _companyService;
        public StockReportController(IStockReportService stockReportService, ICompanyService companyService)
        {
            _stockReportService = stockReportService;
            _companyService = companyService;
        }
        // GET api/stockreport
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/stockreport/5
        public string Get(int id)
        {
            return "value";
        }



        private object GetSLStockXmlElement(IList<BuisnessEntities.Stock.StraightLineStockReportDto> data)
        {
            var xmlfromLINQ = new XElement("slstocks",
            from c in data
            select new XElement("item",
                new XElement("AllocatedBlend", c.AllocatedBlend),
                new XElement("Id", c.Id),
                new XElement("TinNo", c.TinNo),
                new XElement("BoxNo", c.BoxNo),
                new XElement("LotNo", c.LotNo),
                new XElement("Invoice", c.Invoice),
                new XElement("BrokerCode", c.BrokerCode),
                new XElement("GardenMark", c.GardenMark),
                new XElement("SaleCode", c.SaleCode),
                new XElement("SaleDate", c.SaleDate),
                new XElement("SaleType", c.SaleType),
                new XElement("Grade", c.Grade),
                new XElement("Standard", c.Standard),
                new XElement("TeaCertificates", c.TeaCertificates),
                new XElement("Packs", c.Packs),
                new XElement("NetWeight", c.NetWeight),
                new XElement("Rate", c.Rate),
                new XElement("Qty", c.Qty),
                new XElement("Value", c.Value)
                ));
            return xmlfromLINQ;
        }

        

        private object GetBLStockXmlElement(IList<BuisnessEntities.Stock.BlendBalanceStockDto> data)
        {
            var xmlfromLINQ = new XElement("bltocks",
            from c in data
            select new XElement("item",
                new XElement("AllocatedBlend", c.AllocatedBlend),
                new XElement("StockItemId", c.StockItemId),
                new XElement("ItemName", c.ItemName),
                new XElement("Standard", c.Standard),
                new XElement("Status", c.Status),
                new XElement("ClosedDate", c.ClosedDate),
                new XElement("Qty", c.Qty),
                new XElement("Rate", c.Rate),
                new XElement("Value", c.Value)
                //new XElement("BrokerCode", c.Rate)
                ));
            return xmlfromLINQ;
        }

        private HttpResponseMessage GetNoDataPage()
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        public HttpResponseMessage Post([FromBody]StockRptDto obj)
        {
            if (ModelState.IsValid)
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _stockReportService.GetStraighLineStock(obj);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo();
                    var xmlfromLINQ = GetSLStockXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    //var strReportName = obj.IsWithValues == true ? "straightlinestock.rpt" : "straightlinestockstores.rpt";
                    var strReportName = "straightlinestock.rpt";

                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Stock\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("IsWithValue", obj.IsWithValues ? "YES" : "NO");
                    rd.SetParameterValue("IsWithAllocated", obj.IsWithAllocatedLine ? "YES" : "NO");


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

        public HttpResponseMessage Put([FromBody]StockRptDto obj)
        {
            if (ModelState.IsValid)
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _stockReportService.GetBlendBalanceStock(obj.IsWithAllocatedLine);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo();
                    var xmlfromLINQ = GetBLStockXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());
                    //3 : blendstock
                    var strReportName = obj.IsWithValues == true ? "blendstock.rpt" : "blendstockstores.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Stock\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
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
