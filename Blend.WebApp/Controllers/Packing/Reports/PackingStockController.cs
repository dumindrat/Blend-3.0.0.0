using Blend.BuisnessEntities.Packing.Reports;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Packing.Reports;
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

namespace Blend.WebApp.Controllers.Packing.Reports
{
    public class PackingStockController : BaseApi
    {
        private IPackingStockReportService _packindStockSvc;
        private ICompanyService _companyService;
         public PackingStockController(IPackingStockReportService packindStockSvc, ICompanyService companyService)
        {
            _packindStockSvc = packindStockSvc;
            _companyService = companyService;
        }

         private object GetSLStockXmlElement(IList<PackingStockDto> data)
         {
             var xmlfromLINQ = new XElement("slstocks",
             from c in data
             select new XElement("item",
                 new XElement("ItemCode", c.ItemCode),
                 new XElement("ItemName", c.ItemName),
                 new XElement("UOM", c.UOM),
                 new XElement("Qty", c.Qty),
                 new XElement("ReceivedPrice", c.ReceivedPrice),
                 new XElement("Value", c.Value),
                 new XElement("StoreLocation", c.StoreLocation),
                 new XElement("BlendNo", c.BlendNo)
                 ));
             return xmlfromLINQ;
         }

         private HttpResponseMessage GetNoDataPage()
         {

             return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
         }

         public HttpResponseMessage Post([FromBody]StockRptDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                IEnumerable<PackingStockDto> items = new List<PackingStockDto>();
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var data = _packindStockSvc.GetPackingStock(GetCompanyId());
                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetSLStockXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    //var strReportName = obj.IsWithValues == true ? "straightlinestock.rpt" : "straightlinestockstores.rpt";
                    var strReportName = "packingstock.rpt";
                    var sysver = ConfigurationManager.AppSettings["sys_ver"];
                    var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Packing\\Stock\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("IsWithValue", obj.IsWithValues ? "YES" : "NO");
                    //rd.SetParameterValue("IsWithAllocated", obj.IsWithAllocatedLine ? "YES" : "NO");
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);


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
