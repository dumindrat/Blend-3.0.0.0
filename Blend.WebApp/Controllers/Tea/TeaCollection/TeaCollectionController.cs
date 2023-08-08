using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.TeaCollection;
using Blend.BuisnessService.TeaCollection;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.TeaCollection
{
    public class TeaCollectionController : BaseApi
    {
        private ITeaCollectionService _collectionNoteSvc;

        public TeaCollectionController(ITeaCollectionService collectionNoteSvc)
        {
            _collectionNoteSvc = collectionNoteSvc;
        }


        public CollectionNote Get()
        {
            var data = new CollectionNote();
            try
            {
                data = _collectionNoteSvc.GetAllCollectionNote();
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return data;
        }

        [HttpGet]
        [ActionName("GetCollectionNotes")]
        public IEnumerable<CollectionNote> GetCollectionNotes(string id)
        {
            IEnumerable<CollectionNote> data = new List<CollectionNote>();
            try
            {
                id = id.Replace("~", "/");
                data = _collectionNoteSvc.GetAllCollectionNotes(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return data;
        }

        [HttpGet]
        [ActionName("GetStraightLineByBoxLotAndInv")]
        public IEnumerable<StraightLIneCollection> GetStraightLines(string id, string lot, string inv)
        {
            IEnumerable<StraightLIneCollection> data = new List<StraightLIneCollection>();
            try
            {
                id = id.Replace("~", "/");
                lot = lot.Replace("~", "/");
                inv = inv.Replace("~", "/");
                data = _collectionNoteSvc.GetStraightLineByBoxByLotByInv(id, lot, inv);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return data;
        }

        [HttpPost]
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]Blend.BuisnessEntities.TeaBuyer.Buyer value)
        {
            var CollectionNote = new CollectionNote();

            if (ModelState.IsValid && SetUser(CollectionNote))
            {
                try
                {
                    _collectionNoteSvc.CreateCollectionNote(CollectionNote);
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

        [HttpPut]
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([System.Web.Http.FromBody]CollectionNoteItem value)
        {

            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    var collectionNo = value.CollectionNoteNo;
                    collectionNo = collectionNo.Remove(0, 2);
                    value.CollectionNoteId = Convert.ToInt16(collectionNo);
                    _collectionNoteSvc.MarkCollectionNoteItem(value);
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
