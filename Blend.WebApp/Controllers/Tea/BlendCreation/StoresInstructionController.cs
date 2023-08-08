using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.StoresInstruction;
using Blend.BuisnessService.StoresInstruction;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.BlendCreation
{
    public class StoresInstructionController : BaseApi
    {
        private IStoresInstructionService _storesInsService ;
        public StoresInstructionController(IStoresInstructionService storesInsService)
        {
            _storesInsService = storesInsService;
        }

        [HttpGet]
        [ActionName("GetStoresInstructions")]
        public IEnumerable<StoresInstructions> GetStoresInstructions(int id)
        {
            IEnumerable<StoresInstructions> stocks = new List<StoresInstructions>();
            try
            {
                stocks = _storesInsService.GetStoresInstructionsByBlend(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return stocks;
        }

        // POST api/blend
        [DataOperationAuthorization(operationType = "Create")]
        public System.Net.Http.HttpResponseMessage Post([System.Web.Http.FromBody]StoresInstructions ins)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _storesInsService.AddInstruction(ins);
                    return new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        [DataOperationAuthorization(operationType = "Update")]
        public System.Net.Http.HttpResponseMessage Put([System.Web.Http.FromBody]StoresInstructions ins)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _storesInsService.RemoveInstruction(ins.Id);
                    return new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

    }
}
