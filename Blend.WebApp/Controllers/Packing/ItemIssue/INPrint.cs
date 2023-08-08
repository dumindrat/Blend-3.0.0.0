using Blend.BuisnessEntities.Packing.ItemIssue;
using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.BlendCreation;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemIssue;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.PO;
using Blend.BuisnessService.Reports.Blend;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Blend.WebApp.Controllers.Packing.PO
{
    public class INPrint
    {
        private IItemIssueService _itemIssueSvc;
        private IBlendReportService _blSvc;
        private ICompanyService _companyService;

        public INPrint(IItemIssueService itemIssueSvc,
            ICompanyService companyService, ISupplierService supplierService, IBlendReportService blSvc)
        {
            _itemIssueSvc = itemIssueSvc;
            _companyService = companyService;
            _blSvc = blSvc;
        }

        private XElement GetXmlElement(IList<IssueNoteItem> data)
        {
            var xmlfromLINQ = new XElement("items",
              from c in data
              select new XElement("item",
                  new XElement("ItemId", c.ItemId),
                  new XElement("ItemCode", c.ItemCode),
                  new XElement("ItemName", c.ItemName),
                  new XElement("ShortDescription", c.ShortDescription),
                  new XElement("UomName", c.UomName),
                  new XElement("StoreLocation", c.StoreLocation),
                  new XElement("IssueQty", c.Qty),
                  new XElement("Price", c.Price)
                  ));
            return xmlfromLINQ;
        }

        public Stream CreateStream(ItemIssueNoteDto issueNote)
        {
            var store = _companyService.GetStoteInfo(1);
            var company = _companyService.GetCompanyInfo();
            var data = _itemIssueSvc.GetIssueNoteItemById(issueNote.Id);
            var blend = _blSvc.GetBlendSheetParam(issueNote.BlendId);

            var sysver = ConfigurationManager.AppSettings["sys_ver"];
            var syscontact = ConfigurationManager.AppSettings["sys_contact"];
            var xmlfromLINQ = GetXmlElement(data.ToList());
            var textReader = new StringReader(xmlfromLINQ.ToString());

            var strReportName = "in.rpt";
            var rd = new ReportDocument();
            DataSet ds_fldDef = new DataSet();

            string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Packing\\IN\\" + strReportName;
            rd.Load(strPath);
            ds_fldDef.ReadXml(textReader);
            rd.SetDataSource(ds_fldDef);
            rd.SetParameterValue("sys_ver", sysver == null ? "Blend" : sysver);
            rd.SetParameterValue("sys_contact", syscontact == null ? "Achieves" : syscontact);
            rd.SetParameterValue("companyname", company.CompanyName);
            rd.SetParameterValue("Address1", company.Address1 == null ? "" : company.Address1);
            rd.SetParameterValue("Address2", company.Address2 == null ? "" : company.Address2);
            rd.SetParameterValue("Email", company.Email == null ? "" : company.Email);
            rd.SetParameterValue("Phone", company.Telephone == null ? "" : company.Telephone);
            rd.SetParameterValue("BlendNo", issueNote.BlendNo);
            rd.SetParameterValue("Contract", blend.ContractName);
            rd.SetParameterValue("Standard", blend.StandardName);
            //rd.SetParameterValue("SupplierAddress2", supplier.Address2 == null ? "" : supplier.Address2);
            //rd.SetParameterValue("DeliveryPlace", store.StoreName);
            //rd.SetParameterValue("DeliveryAddress1", store.Address1 == null ? "" : store.Address1);
            //rd.SetParameterValue("DeliveryAddress2", store.Address2 == null ? "" : store.Address2);
            //rd.SetParameterValue("DeliveryPhone", store.Telephone == null ? "" : store.Telephone);
            //rd.SetParameterValue("DeliveryContactPerson", store.ContactPerson == null ? "" : store.ContactPerson);
            //rd.SetParameterValue("Deliverydate", po.DeliveryDate == null ? "" : po.DeliveryDate);

            Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
            return ms;
        }
    }
}