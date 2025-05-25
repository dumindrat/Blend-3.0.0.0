using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Contract;
using Blend.BuisnessEntities.Standard;
using Blend.BuisnessService.Contract;

namespace Blend.WebApp.Controllers.Tea.Contracts
{
    public class ContractDetailController : BaseApi
    {
        private IContractService _contractSvc;
        public ContractDetailController(IContractService contractSvc)
        {
            _contractSvc = contractSvc;
        }

        [HttpGet]
        [ActionName("GetBlendsByContractAndStandard")]
        public IEnumerable<BlendNoByCon> GetBlendsByContractAndStandard(int contractId, int standardId)
        {
            IEnumerable<BlendNoByCon> standards = new List<BlendNoByCon>();
            if (ModelState.IsValid)
            {
                try
                {
                    standards = _contractSvc.GetBlendListByConAndStanList(contractId, standardId);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return standards;
        }

        // GET api/contractdetail
        [HttpGet]
        [ActionName("GetAvailableStandards")]
        public IEnumerable<TeaStandardForContract> Get(int id, string standard)
        {
            IList<TeaStandardForContract> standards = new List<TeaStandardForContract>();
            if (ModelState.IsValid)
            {
                try
                {
                    standards = _contractSvc.GetAvailableStandardsByIdAndSearchStr(id, standard, GetCompanyId());
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return standards;
        }

        // GET api/contract/5
        [HttpGet]
        [ActionName("GetBuyerContractDetails")]
        public IEnumerable<BuyerContractDetail> GetBuyerContractDetails(int id)
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
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return addedStandards;
        }

        // GET api/contract/5
        [HttpGet]
        [ActionName("GetStandardByContractName")]
        public IEnumerable<BuyerContractDetail> GetStandardByContractName(string id)
        {
            IEnumerable<BuyerContractDetail> addedStandards = new List<BuyerContractDetail>();
            if (ModelState.IsValid)
            {
                try
                {
                    var contractId = id.Split('|').Count() > 1 ? id.Split('|').ToList()[1] : "0";
                    addedStandards = _contractSvc.GetBuyerContractDetailById(Convert.ToInt16(contractId));
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return addedStandards;
                }
            }
            return addedStandards;
        }

        // POST api/contractdetail
        public HttpResponseMessage Post([FromBody]BuyerContractDetail contractDet)
        {
            if (ModelState.IsValid && SetUser(contractDet))
            {
                try
                {
                    _contractSvc.CreateBuyerContractDetail(contractDet);
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

        public HttpResponseMessage Put([FromBody]BuyerContractDetail contractDet)
        {
            if (ModelState.IsValid && SetUser(contractDet))
            {
                try
                {
                    _contractSvc.UpdateBuyerContractDetail(contractDet);
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



        // DELETE api/contractdetail/5
        [HttpDelete]
        public HttpResponseMessage Delete(int contractId, int standardId, byte[] version)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _contractSvc.DeleteBuyerContractDetail(new BuyerContractDetail { ContractId = contractId, StandardId = standardId, RowVersion = version });
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
