using Blend.BuisnessEntities.CompanyRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Company
{
    public interface ICompanyService
    {
        CompanyDto GetCompanyInfo();
        StoresDto GetStoteInfo(int id);
    }
}
