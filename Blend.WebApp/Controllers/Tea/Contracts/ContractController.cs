using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Contract;
using Blend.BuisnessService.Contract;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.Contracts
{
    public class ContractController : BaseApi
    {
        //private IOperation<BuyerContract> dataManager;
        private IContractService _contractSvc;

        public ContractController(IContractService contractSvc)
        {
            //dataManager = new BuyerContractManagement(  );
            _contractSvc = contractSvc;
        }

        [HttpGet]
        [ActionName("GetAllContracts")]
        public IEnumerable<BuyerContract> GetAllContracts(string id)
        {
            IEnumerable<BuyerContract> contracts = new List<BuyerContract>();
            if (ModelState.IsValid)
            {
                try
                {
                    contracts = _contractSvc.GetContractList(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                }
            }
            return contracts;
        }

        public IEnumerable<BuyerContract> Get()
        {
            IEnumerable<BuyerContract> contracts = new List<BuyerContract>();
            if (ModelState.IsValid)
            {
                try
                {
                    contracts = _contractSvc.GetOpenContractList();
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                }
            }
            return contracts;
        }
        // GET api/contract/5
        public IEnumerable<BuyerContractDetail> Get(int id)
        {
            IEnumerable<BuyerContractDetail> addedStandards = new List<BuyerContractDetail>();
            if (ModelState.IsValid)
            {
                try
                {
                    addedStandards = _contractSvc.GetBuyerContractDetailById(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                }
            }
            return addedStandards;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]BuyerContract contract)
        {
            if (ModelState.IsValid && SetUser(contract))
            {
                try
                {
                    _contractSvc.CreateContract(contract);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]BuyerContract value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _contractSvc.UpdateContract(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }


        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _contractSvc.DeleteContract(new BuyerContract { ContractId = id });
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
