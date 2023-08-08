using Blend.BuisnessEntities.Email;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.ItemReturn;
using Blend.BuisnessService.Packing.PO;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.PO
{
    public class ItemReturnSendMailController : BaseApi
    {
        public IEmailService _emailService;
        private IPurchaseOrderService _poSvc;
        private ICompanyService _companyService;
        private ISupplierService _supplierService;
        public IItemRetunService _itemReturnSvc;
        private IItemReceiveService _itemReceiveSvc;

        public ItemReturnSendMailController(IEmailService emailService, IPurchaseOrderService poSvc, ICompanyService companyService,
            ISupplierService supplierService, IItemRetunService itemReturnSvc, IItemReceiveService itemReceiveSvc)
        {
            _emailService = emailService;
            _poSvc = poSvc;
            _companyService = companyService;
            _supplierService = supplierService;
            _itemReturnSvc = itemReturnSvc;
            _itemReceiveSvc = itemReceiveSvc;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]EmailDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    var sPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["PoUploadPath"]);
                    value.CC = new List<string>();
                    value.CC.Add(ConfigurationManager.AppSettings["PoCC"]);

                    var rtn = _itemReturnSvc.GetReturNoteById(value.ReferenceId);
                    var reportStream = new ItemReturnPrint(_poSvc, _companyService, _supplierService, _itemReturnSvc, _itemReceiveSvc).CreateStream(rtn);
                    var fileName = rtn.ReturnNoteNo + "-" + DateTime.Now.ToString() + ".pdf";
                    fileName = fileName.Replace('/', '-').Replace(':', '-');
                    var formatFile = Path.GetFileName(fileName);
                    var filePath = sPath + fileName;
                    //Path.GetFileName(fileName);

                    var fileStream = File.Create(filePath);
                    reportStream.Seek(0, SeekOrigin.Begin);
                    reportStream.CopyTo(fileStream);
                    fileStream.Close();
                    value.Attachment = filePath;
                    _emailService.SendMail(value);

                    var emailHistory = new EmailHistory();
                    emailHistory.MenuId = 47;
                    emailHistory.ReferenceId = rtn.Id;
                    emailHistory.ReferenceNo = rtn.ReturnNoteNo;
                    emailHistory.FromAddress = value.From;
                    emailHistory.ToAddress = value.To;
                    emailHistory.CC = ConfigurationManager.AppSettings["PoCC"];
                    emailHistory.EmailBody = value.Message;
                    emailHistory.Subject = value.Subject;
                    emailHistory.AttachmentPath = filePath;
                    SetUser(emailHistory);

                    _emailService.SaveEmailHistory(emailHistory);

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        [ActionName("GetEmailHistory")]
        public IEnumerable<EmailHistory> GetEmailHistory(string id)
        {
            IEnumerable<EmailHistory> emails = new List<EmailHistory>();

            if (ModelState.IsValid)
            {
                try
                {
                    emails = _emailService.GetEmailList(Convert.ToInt16(id), 47);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return emails;

        }
    }
}
