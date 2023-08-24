using Blend.BuisnessEntities.CompanyRegistration;
using Blend.BuisnessService.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Company
{
    public class CompanyController : BaseApi
    {
        private ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        // GET api/company
        //public CompanyDto Get()
        //{
        //    return _companyService.GetCompanyInfo();
        //}

        [HttpGet]
        [ActionName("GetUserCompanies")]
        public IEnumerable<CompanyDto> Get(string id)
        {
            IEnumerable<CompanyDto> userCompanies = new List<CompanyDto>();
            try
            {
                id = id.Replace("~", "/");
                userCompanies = _companyService.GetUserCompanyInfo(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return userCompanies;
            }
            return userCompanies;
        }

        [HttpGet]
        [ActionName("GetDefaultUserCompany")]
        public CompanyDto GetDefaultUserCompany(string id, string status)
        {
            CompanyDto userCompanies = new CompanyDto();
            try
            {
                id = id.Replace("~", "/");
                userCompanies = _companyService.GetDefaultUserCompanyInfo(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return userCompanies;
            }
            return userCompanies;
        }

        [HttpPost]
        [ActionName("SetAsDefaultUserCompany")]
        public HttpResponseMessage Post([FromBody]UserCompanyDto userCompany)
        {
            if (ModelState.IsValid && SetUser(userCompany))
            {
                try
                {
                    _companyService.SetAsDefaultCompany(userCompany);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        //// POST api/company
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/company/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/company/5
        //public void Delete(int id)
        //{
        //}
    }
}
