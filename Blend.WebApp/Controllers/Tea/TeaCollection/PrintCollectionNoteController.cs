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
using Blend.BuisnessEntities.TeaCollection;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.TeaCollection;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Blend.WebApp.Controllers.Tea.TeaCollection
{
    public class PrintCollectionNoteController : ApiController
    {
        private ITeaCollectionService _collectionNoteSvc;
        private ICompanyService _companyService;

        public PrintCollectionNoteController(ITeaCollectionService collectionNoteSvc, ICompanyService companyService)
        {
            _collectionNoteSvc = collectionNoteSvc;
            _companyService = companyService;
        }

        private HttpResponseMessage GetNoDataPage()
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        [HttpGet]
        [ActionName("GetCollectionNotePrint")]
        public HttpResponseMessage GetCollectionNotePrint(string id, string download)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);

            if (ModelState.IsValid)
            {
                try
                {
                    var noteNo = id.Substring(2, id.Length - 2);
                    var data = _collectionNoteSvc.GetCollectionNotePrint(Convert.ToInt16(noteNo));
                    var sysver = ConfigurationManager.AppSettings["sys_ver"];
                    var syscontact = ConfigurationManager.AppSettings["sys_contact"];

                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo();
                    var xmlfromLINQ = GetXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "collectionnote.rpt";
                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();
                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Purchase\\Collection\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("collectionnoteno", id);
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                    Stream ms = null ;
                    var mediaType = "application/pdf";

                    if (download == "true")
                    {
                        ms = rd.ExportToStream(ExportFormatType.Excel);
                        mediaType = "application/vnd.ms-excel";
                    }
                    else
                    {
                        ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    }
                    result.Content = new StreamContent(ms);
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                    result.Content.Headers.ContentDisposition.FileName = strReportName;
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

                }
                catch (Exception ex)
                {
                    return result;
                }
            }
            return result;

        }

        private XElement GetXmlElement(IList<CollectionReportDto> data)
        {
            var xmlfromLINQ = new XElement("collectionitems",
              from c in data
              select new XElement("item",
                  new XElement("BoxNo", c.BoxNo),
                  new XElement("LotNo", c.LotNo),
                  new XElement("Invoice", c.Invoice),
                  new XElement("BrokerCode", c.BrokerCode),
                  new XElement("SaleCode", c.SaleCode),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("SaleType", c.SaleType),
                  new XElement("Grade", c.Grade),
                  new XElement("GardenMark", c.GardenMark),
                  new XElement("Packs", c.Packs),
                  new XElement("NetWeight", c.NetWeight),
                  new XElement("Weight", c.Weight),
                  new XElement("CollectionDate", c.CollectionDate),
                  new XElement("StockReceivedDate", c.StockReceivedDate),
                  new XElement("StockReceived", c.StockReceived),
                  new XElement("StoreLocation", c.StoreLocation)

                  ));
            return xmlfromLINQ;
        }


    }
}
