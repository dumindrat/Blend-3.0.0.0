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
using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Broker;
using Blend.BuisnessService.Company;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Blend.WebApp.Controllers.Tea.Report.Reject_Short
{
    public class ShortNoteController : BaseApi
    {

        private ICompanyService _companyService;
        private IBrokerService _brokerSvc;
        public ShortNoteController(ICompanyService companyService, IBrokerService brokerSvc)
        {
            _companyService = companyService;
            _brokerSvc = brokerSvc;
        }

        private XElement GetXmlElement(IList<PrintNoteDto> data)
        {
            var xmlfromLINQ = new XElement("blenditems",
              from c in data
              select new XElement("item",
                  new XElement("ItemName", c.ItemName),
                  new XElement("LotNo", c.LotNo),
                  new XElement("Invoice", c.Invoice),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("SaleType", c.SaleType),
                  new XElement("SaleCode", c.SaleCode),
                  new XElement("GardenMark", c.GardenMark),
                  new XElement("Grade", c.Grade),
                  new XElement("Qty", c.Qty),
                  new XElement("Rate", c.Rate)

                  ));
            return xmlfromLINQ;
        }


        private HttpResponseMessage GetNoDataPage()
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        public HttpResponseMessage Post([FromBody]NotePrint obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var sysver = ConfigurationManager.AppSettings["sys_ver"];
                    var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                    if (obj.BoxNoList.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var brokerInfo = _brokerSvc.GetBrokerById(obj.BrokerId); 

                    var xmlfromLINQ = GetXmlElement(obj.BoxNoList.Where(c => c.IsSelected == true).ToList());

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = "shortnote.rpt";

                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Letters\\ShortNote\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                    rd.SetParameterValue("broker", brokerInfo.BrokerName);
                    rd.SetParameterValue("forwhom", obj.ForWhom);
                    rd.SetParameterValue("authorizedperson", obj.AuthorizedPerson);
                    rd.SetParameterValue("Address1", brokerInfo.Address);


                    Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
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
