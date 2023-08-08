using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.PO;
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
    public class IRPrint
    {
        private IItemReceiveService _itemReceiveSvc;
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;

        public IRPrint(IItemReceiveService itemReceiveSvc, 
            ICompanyService companyService, ISupplierService supplierService,IPurchaseOrderService poSvc)
        {
            _itemReceiveSvc = itemReceiveSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _poSvc = poSvc;
        }

        private XElement GetXmlElement(IList<ReceiveNoteItems> data)
        {
            var xmlfromLINQ = new XElement("items",
              from c in data
              select new XElement("item",
                  new XElement("ItemId", c.ItemId),
                  new XElement("ItemName", c.ItemName),
                  new XElement("ShortDescription", c.ShortDescription),
                  new XElement("UOM", c.UomName),
                  new XElement("OrderQty", c.OrderQty),
                  new XElement("OrderPrice", c.OrderPrice),
                  new XElement("ReceivedQty", c.ReceivedQty),
                  new XElement("ReceivedPrice", c.ReceivedPrice),
                  new XElement("ReceivedDate", c.ReceivedDate),
                  new XElement("ItemCode", c.ItemCode),
                  new XElement("Comment", c.Comment)

                  ));
            return xmlfromLINQ;
        }

        public Stream CreateStream(ItemReceiveNoteDto receiveNote)
        {
            var store = _companyService.GetStoteInfo(1);
            var company = _companyService.GetCompanyInfo();
            var data = _itemReceiveSvc.GetReceiveNoteItems(receiveNote.Id);
            var po = _poSvc.GetPOById(receiveNote.PoId);
            var supplier = _supplierService.GetSupplierById(po.SupplierId);

            var sysver = ConfigurationManager.AppSettings["sys_ver"];
            var syscontact = ConfigurationManager.AppSettings["sys_contact"];
            var xmlfromLINQ = GetXmlElement(data.ToList());
            var textReader = new StringReader(xmlfromLINQ.ToString());

            var strReportName = "ir.rpt";
            var rd = new ReportDocument();
            DataSet ds_fldDef = new DataSet();

            string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Packing\\IR\\" + strReportName;
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
            rd.SetParameterValue("PONumber", po.PONumber);
            rd.SetParameterValue("SupplierName", po.Supplier);
            rd.SetParameterValue("SupplierAddress1", supplier.Address1 == null ? "" : supplier.Address1);
            rd.SetParameterValue("SupplierAddress2", supplier.Address2 == null ? "" : supplier.Address2);
            rd.SetParameterValue("DeliveryPlace", store.StoreName);
            rd.SetParameterValue("DeliveryAddress1", store.Address1 == null ? "" : store.Address1);
            rd.SetParameterValue("DeliveryAddress2", store.Address2 == null ? "" : store.Address2);
            rd.SetParameterValue("DeliveryPhone", store.Telephone == null ? "" : store.Telephone);
            rd.SetParameterValue("DeliveryContactPerson", store.ContactPerson == null ? "" : store.ContactPerson);
            rd.SetParameterValue("Deliverydate", po.DeliveryDate == null ? "" : po.DeliveryDate);

            Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
            return ms;
        }
    }
}