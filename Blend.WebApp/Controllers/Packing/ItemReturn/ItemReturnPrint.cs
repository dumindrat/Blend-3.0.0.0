using Blend.BuisnessEntities.Packing.ItemReturn;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.ItemReturn;
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
    public class ItemReturnPrint
    {
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;
        public IItemRetunService _itemReturnSvc;
        private IItemReceiveService _itemReceiveSvc;

        public ItemReturnPrint(IPurchaseOrderService poSvc, 
            ICompanyService companyService, 
            ISupplierService supplierService,
            IItemRetunService itemReturnSvc,
            IItemReceiveService itemReceiveSvc)
        {
            _poSvc = poSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _itemReturnSvc = itemReturnSvc;
            _itemReceiveSvc = itemReceiveSvc;
        }

        private XElement GetXmlElement(IList<ReturnNoteItems> data)
        {
            var xmlfromLINQ = new XElement("items",
              from c in data
              select new XElement("item",
                  new XElement("ItemId", c.ItemId),
                  new XElement("ItemName", c.ItemName),
                  new XElement("ShortDescription", c.ShortDescription),
                  new XElement("UOM", c.UomName),
                  new XElement("Qty", c.ReturnQty),
                  new XElement("Price", c.Price),
                  new XElement("Reason", c.Reason)

                  ));
            return xmlfromLINQ;
        }

        public Stream CreateStream(ItemReturnNoteDto rn)
        {
            var store = _companyService.GetStoteInfo(1);
            var company = _companyService.GetCompanyInfo();
            var data = _itemReturnSvc.GetReturnNoteItemById(rn.Id);
            var receivenote = _itemReceiveSvc.GetReceiveNoteById(rn.ReceiveNoteId);
            var po = _poSvc.GetPOById(receivenote.PoId);
            var supplier = _supplierService.GetSupplierById(po.SupplierId);

            var sysver = ConfigurationManager.AppSettings["sys_ver"];
            var syscontact = ConfigurationManager.AppSettings["sys_contact"];
            var xmlfromLINQ = GetXmlElement(data.ToList());
            var textReader = new StringReader(xmlfromLINQ.ToString());

            var strReportName = "rn.rpt";
            var rd = new ReportDocument();
            DataSet ds_fldDef = new DataSet();

            string strPath = HttpContext.Current.Server.MapPath("~/") + "Reports\\Packing\\RN\\" + strReportName;
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
            rd.SetParameterValue("Instructions", rn.Comment == null ? "" : rn.Comment);
            rd.SetParameterValue("Deliverydate", po.DeliveryDate == null ? "" : po.DeliveryDate);
            rd.SetParameterValue("GRNNo", receivenote.ReceiveNoteNo);

            Stream ms = rd.ExportToStream(ExportFormatType.PortableDocFormat);
            return ms;
        }
    }
}