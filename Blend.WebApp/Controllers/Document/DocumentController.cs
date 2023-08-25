using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.TransDocument;
using Blend.BuisnessService.Document;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Document
{
    public class DocumentController : BaseApi
    {
        private IDocumentService _docService;

        public DocumentController(IDocumentService docService)
        {
            _docService = docService;
        }
        // GET api/document
        public IEnumerable<DocumentNo> Get()
        {
            return _docService.GetDocumentData();
        }

        public DocumentNo GetDocumentNo(int id)
        {
            return _docService.GetDocumentNo(id, GetCompanyId());
        }
        // GET api/document/5
        public IEnumerable<DocumentNo> GetDocumentNosByModule(int id)
        {
            try
            {
                return _docService.GetDocumentDataByModule(id, GetCompanyId());
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetDocumentData fails. Error " + ex.Message);
                return new List<DocumentNo>();
            }
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]Blend.BuisnessEntities.TransDocument.DocumentNo value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _docService.ResetDocumentNo(value);
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


    }
}
