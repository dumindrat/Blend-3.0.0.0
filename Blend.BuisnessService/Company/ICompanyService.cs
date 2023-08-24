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
        IList<UserCompanyDto> GetUserCompanyInfo(string userName);
        UserCompanyDto GetDefaultUserCompanyInfo(string userName);
        StoresDto GetStoteInfo(int id);
        bool SetAsDefaultCompany(UserCompanyDto userCompany);
    }
}
