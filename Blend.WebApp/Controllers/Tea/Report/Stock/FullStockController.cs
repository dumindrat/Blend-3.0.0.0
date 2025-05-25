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

namespace Blend.WebApp.Controllers.Tea.Report.Stock
{
    public class FullStockController : BaseApi
    {
        private IStockReportService _stockReportService;
        private ICompanyService _companyService;

        public FullStockController(IStockReportService stockReportService, ICompanyService companyService)
        {
            _stockReportService = stockReportService;
            _companyService = companyService;
        }


        public HttpResponseMessage Post([FromBody]StockRptDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _stockReportService.GetStraighLineAndBlendBalanceStock(obj);
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    dynamic xmlfromLINQ = "";
                    if (obj.IsWithRejectAndShortLine)
                    {
                        xmlfromLINQ = GetSLStockXmlElement(data);
                    }
                    else
                    {
                        xmlfromLINQ = GetSLStockXmlElement(data.Where(c => c.SeqNo == 1 || c.SeqNo == 4 || c.SeqNo == 5 || c.SeqNo == 6).ToList());

                    }
                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "fullstock.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Stock\\FullStock\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("IsWithValue", obj.IsWithValues == true ? "YES" : "NO");

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

        private HttpResponseMessage GetNoDataPage()
        {

            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        private object GetSLStockXmlElement(IList<BuisnessEntities.Stock.FullStockReportDto> data)
        {
            var xmlfromLINQ = new XElement("slstocks",
            from c in data
            select new XElement("item",
                new XElement("SeqNo", c.SeqNo),
                new XElement("StockType", c.StockType),
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
    }
}
