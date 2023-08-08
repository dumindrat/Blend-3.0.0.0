using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Blend.BuisnessEntities.TeaCollection;
using Blend.BuisnessService.TeaCollection;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.TeaCollection
{
    public class TeaLocationController : BaseApi
    {
        private ITeaCollectionService _collectionNoteSvc;

        public TeaLocationController(ITeaCollectionService collectionNoteSvc)
        {
            _collectionNoteSvc = collectionNoteSvc;
        }

        [HttpPost]
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]CollectionNoteItem value)
        {

            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _collectionNoteSvc.MarkCollectionNoteStoreLocation(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

    }
}
