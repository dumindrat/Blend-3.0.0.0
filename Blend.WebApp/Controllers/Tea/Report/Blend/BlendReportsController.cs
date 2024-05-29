using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.Report;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Blend;
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

namespace Blend.WebApp.Controllers.Tea.Report.Blend
{
    public class BlendReportsController : BaseApi
    {
        private IBlendReportService _blendRptService;
        private ICompanyService _companyService;
        public BlendReportsController(IBlendReportService blendRptService, ICompanyService companyService)
        {
            _blendRptService = blendRptService;
            _companyService = companyService;
        }

        // GET api/blendreports
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
        [ActionName("GetBlendSheet")]
        public HttpResponseMessage GetBlendSheet(int id)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var sysver = ConfigurationManager.AppSettings["sys_ver"];
                var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                var data = _blendRptService.GetBlendSheet(id);
                var param = _blendRptService.GetBlendSheetParam(id);
                var lineSum = data.Where(c => c.ItemType == 1).Sum(c => c.Qty);
                var linevalue = data.Where(c => c.ItemType == 1).Sum(c => c.Value);
                var exportQty = Convert.ToDecimal(param.ExportQty);
                var BlenSum = data.Where(c => c.ItemType == 2).Sum(c => c.Qty);
                var blendvalue = data.Where(c => c.ItemType == 2).Sum(c => c.Value);

                if (data.Count == 0)
                    return GetNoDataPage();
                var companyInfo = _companyService.GetCompanyInfo(GetCompanyId());
                var xmlfromLINQ = GetXmlElement(data);

                var textReader = new StringReader(xmlfromLINQ.ToString());

                var strReportName = "blendreport.rpt";
                var rd = new ReportDocument();
                DataSet ds_fldDef = new DataSet();

                string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Blendsheet\\" + strReportName;
                rd.Load(strPath);
                ds_fldDef.ReadXml(textReader);
                rd.SetDataSource(ds_fldDef);
                rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                rd.SetParameterValue("tcepr", companyInfo.TCEPR == null ? "" : companyInfo.TCEPR);
                rd.SetParameterValue("contract", param.ContractName == null ? "" : param.ContractName);
                rd.SetParameterValue("standard", param.StandardName == null ? "" : param.StandardName);
                rd.SetParameterValue("blendno", param.BlendNo == null ? "" : param.BlendNo);
                rd.SetParameterValue("datecompletion", param.DateOfCompletion == null ? "" : param.DateOfCompletion);
                rd.SetParameterValue("avgprice", param.ClosedRate == null ? "" : param.ClosedRate);
                rd.SetParameterValue("qtytobeexported", param.ExportQty == null ? "" : param.ExportQty);
                rd.SetParameterValue("bg", param.BG == null ? "" : param.BG);
                rd.SetParameterValue("bb", param.BB == null ? String.Format("{0:0.00}", lineSum + BlenSum - exportQty) : String.Format("{0:0.00}", param.BB));
                rd.SetParameterValue("bl", param.BL == null ? "" : param.BL);
                rd.SetParameterValue("totallineqty", String.Format("{0:0.00}", lineSum));
                rd.SetParameterValue("totalblendbalanceqty", String.Format("{0:0.00}", BlenSum));
                rd.SetParameterValue("totallinevalue", String.Format("{0:0.00}", linevalue));
                rd.SetParameterValue("totalblendbalancevalue", String.Format("{0:0.00}", blendvalue));
                rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);


                Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                result.Content = new StreamContent(ms);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return result;

        }

        [HttpGet]
        [ActionName("GetStoresSheet")]
        public HttpResponseMessage GetStoresSheet(int id)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var sysver = ConfigurationManager.AppSettings["sys_ver"];
                var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                var data = _blendRptService.GetBlendSheet(id);
                var param = _blendRptService.GetBlendSheetParam(id);
                var lineSum = data.Where(c => c.ItemType == 1).Sum(c => c.Qty);
                var linevalue = data.Where(c => c.ItemType == 1).Sum(c => c.Value);

                var BlenSum = data.Where(c => c.ItemType == 2).Sum(c => c.Qty);
                var blendvalue = data.Where(c => c.ItemType == 2).Sum(c => c.Value);

                if (data.Count == 0)
                    return GetNoDataPage();
                var companyInfo = _companyService.GetCompanyInfo(GetCompanyId());
                var xmlfromLINQ = GetXmlElement(data);

                var textReader = new StringReader(xmlfromLINQ.ToString());

                var strReportName = "storessheet.rpt";
                var rd = new ReportDocument();
                DataSet ds_fldDef = new DataSet();

                string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Blendsheet\\VN\\" + strReportName;
                rd.Load(strPath);
                ds_fldDef.ReadXml(textReader);
                rd.SetDataSource(ds_fldDef);
                rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                rd.SetParameterValue("tcepr", companyInfo.TCEPR == null ? "" : companyInfo.TCEPR);
                rd.SetParameterValue("contract", param.ContractName == null ? "" : param.ContractName);
                rd.SetParameterValue("standard", param.StandardName == null ? "" : param.StandardName);
                rd.SetParameterValue("blendno", param.BlendNo == null ? "" : param.BlendNo);
                rd.SetParameterValue("datecompletion", param.DateOfCompletion == null ? "" : param.DateOfCompletion);
                rd.SetParameterValue("avgprice", param.ClosedRate == null ? "" : param.ClosedRate);
                rd.SetParameterValue("qtytobeexported", param.ExportQty == null ? "" : param.ExportQty);
                rd.SetParameterValue("bg", param.BG == null ? "" : param.BG);
                rd.SetParameterValue("bb", param.BB == null ? "" : param.BB);
                rd.SetParameterValue("bl", param.BL == null ? "" : param.BL);
                rd.SetParameterValue("totallineqty", String.Format("{0:0.00}", lineSum));
                rd.SetParameterValue("totalblendbalanceqty", String.Format("{0:0.00}", BlenSum));
                rd.SetParameterValue("totallinevalue", String.Format("{0:0.00}", linevalue));
                rd.SetParameterValue("totalblendbalancevalue", String.Format("{0:0.00}", blendvalue));
                rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                rd.SetParameterValue("trialversion", param.IsClosed ? "Closed" : param.TrialVersion);


                Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                result.Content = new StreamContent(ms);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return result;

        }

        [HttpGet]
        [ActionName("GetBlendCanister")]
        public HttpResponseMessage GetBlendCanister(int id)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var sysver = ConfigurationManager.AppSettings["sys_ver"];
                var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                var data = _blendRptService.GetBlendCanister(id);

                if (data.Count == 0)
                    return GetNoDataPage();
                var companyInfo = _companyService.GetCompanyInfo(GetCompanyId());
                var xmlfromLINQ = GetCanisterXmlElement(data);

                var textReader = new StringReader(xmlfromLINQ.ToString());

                var strReportName = "canistersheet.rpt";
                var rd = new ReportDocument();
                DataSet ds_fldDef = new DataSet();

                string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Blendsheet\\" + strReportName;
                rd.Load(strPath);
                ds_fldDef.ReadXml(textReader);
                rd.SetDataSource(ds_fldDef);
                //rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                //rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);


                Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                result.Content = new StreamContent(ms);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
                result.Content.Headers.ContentDisposition.FileName = strReportName;
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return result;

        }

        private XElement GetXmlElement(IList<BlendSheetDto> data)
        {
            var xmlfromLINQ = new XElement("blenditems",
              from c in data
              select new XElement("item",
                  new XElement("Id", c.Id),
                  new XElement("ItemName", c.ItemName),
                  new XElement("ItemType", c.ItemType),
                  new XElement("LotNo", c.LotNo),
                  new XElement("Invoice", c.Invoice),
                  new XElement("BrokerCode", c.BrokerCode),
                  new XElement("SaleCode", c.SaleCode),
                  new XElement("SaleDate", c.SaleDate),
                  new XElement("SaleType", c.SaleType),
                  new XElement("Standard", c.Standard),
                  new XElement("Grade", c.Grade),
                  new XElement("GardenMark", c.GardenMark),
                  new XElement("Packs", c.Packs),
                  new XElement("NetWeight", c.NetWeight),
                  new XElement("Rate", c.Rate),
                  new XElement("Qty", c.Qty),
                  new XElement("TinNo", c.TinNo),
                  new XElement("TeaCertificates", c.TeaCertificates)

                  ));
            return xmlfromLINQ;
        }

        private XElement GetCanisterXmlElement(IList<BlendCanister> data)
        {
            var xmlfromLINQ = new XElement("blenditems",
              from c in data
              select new XElement("item",
                  new XElement("BlendNo", c.BlendNo),
                  new XElement("Standard", c.Standard),
                  new XElement("StockType", c.StockType),
                  new XElement("StockQty", c.StockQty),
                  new XElement("Rate", c.Rate)
                  ));
            return xmlfromLINQ;
        }


        private HttpResponseMessage GetNoDataPage()
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, "No Data Founf");
        }

        public HttpResponseMessage Post([FromBody]BlendSheetRptDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                var result = Request.CreateResponse(HttpStatusCode.OK);
                try
                {
                    var sysver = ConfigurationManager.AppSettings["sys_ver"];
                    var syscontact = ConfigurationManager.AppSettings["sys_contact"];
                    var data = _blendRptService.GetBlendSheet(obj.BlendId);
                    var param = _blendRptService.GetBlendSheetParam(obj.BlendId);
                    var exportQty = Convert.ToDecimal(param.ExportQty);
                    var closedBal = Convert.ToDecimal(param.BB) + Convert.ToDecimal(param.BG);
                    var lineSum = data.Where(c => c.ItemType == 1).Sum(c => c.Qty);
                    var linevalue = data.Where(c => c.ItemType == 1).Sum(c => c.Value);

                    var BlenSum = data.Where(c => c.ItemType == 2).Sum(c => c.Qty);
                    var blendvalue = data.Where(c => c.ItemType == 2).Sum(c => c.Value);
                    var blendBal = param.IsClosed == false ? String.Format("{0:0.00}", lineSum + BlenSum - exportQty) : String.Format("{0:0.00}", closedBal);
                    var blendgain = param.BG == null ? "0" : param.BG;
                    var blendloss = param.BL == null ? "0" : param.BL;

                    //Avg Calculation for full decimal places
                    var totLineQty = lineSum + BlenSum;
                    //var finalQty = Convert.ToDecimal(param.BG) > 0 ? Convert.ToDecimal(param.BG) + totLineQty : totLineQty - Convert.ToDecimal(param.BL);
                    var avgPrice = (blendvalue + linevalue) / totLineQty;

                    if (data.Count == 0)
                        return GetNoDataPage();
                    var companyInfo = _companyService.GetCompanyInfo(obj.CompanyId);
                    var xmlfromLINQ = GetXmlElement(data);

                    var textReader = new StringReader(xmlfromLINQ.ToString());

                    var strReportName = obj.IsWithValues == true ? "blendreport.rpt" : "blendreportstores.rpt";

                    var rd = new ReportDocument();
                    DataSet ds_fldDef = new DataSet();

                    string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Blendsheet\\" + strReportName;
                    rd.Load(strPath);
                    ds_fldDef.ReadXml(textReader);
                    rd.SetDataSource(ds_fldDef);
                    rd.SetParameterValue("companyname", companyInfo.CompanyName == null ? "" : companyInfo.CompanyName);
                    rd.SetParameterValue("tcepr", companyInfo.TCEPR == null ? "" : companyInfo.TCEPR);
                    rd.SetParameterValue("contract", param.ContractName == null ? "" : param.ContractName);
                    rd.SetParameterValue("standard", param.StandardName == null ? "" : param.StandardName);
                    rd.SetParameterValue("blendno", param.BlendNo == null ? "" : param.BlendNo);
                    rd.SetParameterValue("datecompletion", param.DateOfCompletion == null ? "" : param.DateOfCompletion);
                    rd.SetParameterValue("avgprice", param.IsClosed ? param.ClosedRate : String.Format("{0:0.0000}", avgPrice));
                    rd.SetParameterValue("qtytobeexported", param.ExportQty == null ? "" : param.ExportQty);
                    rd.SetParameterValue("bg", param.IsClosed ? blendgain : "0");
                    rd.SetParameterValue("bb", blendBal);
                    rd.SetParameterValue("bl", param.IsClosed ? blendloss : "0");
                    rd.SetParameterValue("totallineqty", String.Format("{0:0.00}", lineSum));
                    rd.SetParameterValue("totalblendbalanceqty", String.Format("{0:0.00}", BlenSum));
                    rd.SetParameterValue("totallinevalue", String.Format("{0:0.00}", linevalue));
                    rd.SetParameterValue("totalblendbalancevalue", String.Format("{0:0.00}", blendvalue));
                    rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
                    rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
                    //rd.SetParameterValue("iswithwatermark", obj.IsWithWaterMark == true ? "YES" : "NO");
                    rd.SetParameterValue("trialversion", param.IsClosed ? "Closed" : param.TrialVersion);

                    var mediaType = "application/pdf";

                    Stream ms = null;
                    //ms = obj.FileType ? rd.ExportToStream(ExportFormatType.Excel) : rd.ExportToStream(ExportFormatType.PortableDocFormat);
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
