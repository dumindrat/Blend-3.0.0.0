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
    public class CompanyController : ApiController
    {
        private ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        // GET api/company
        public CompanyDto Get()
        {
            return _companyService.GetCompanyInfo();
        }

        // GET api/company/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/company
        public void Post([FromBody]string value)
        {
        }

        // PUT api/company/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/company/5
        public void Delete(int id)
        {
        }
    }
}
