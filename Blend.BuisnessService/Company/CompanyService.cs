using Blend.BuisnessEntities.CompanyRegistration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Company
{
    public class CompanyService : BaseManagement<CompanyDto>, ICompanyService
    {
        public CompanyDto GetCompanyInfo()
        {
            try
            {
                var mapper = new DataMapper<CompanyDto>("GetCompanyInfo", new Hashtable() { });

                var result = mapper.Object();// Mapper.QueryForObject<CompanyDto>("GetCompanyInfo", new Hashtable() { });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetCompanyInfo fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public StoresDto GetStoteInfo(int id)
        {
            try
            {
                var mapper = new DataMapper<StoresDto>("GetStoresInfo", new Hashtable() { });

                var result = mapper.ObjectList();
                var store = result.Where(c => c.StoreId == id);
                return result.Single();
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetCompanyInfo fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public IList<UserCompanyDto> GetUserCompanyInfo(string userName)
        {
            try
            {
                var mapper = new DataMapper<UserCompanyDto>("GetUserCompanyInfo", new Hashtable() { { "UserName", userName } });

                var result = mapper.ObjectList();// Mapper.QueryForObject<CompanyDto>("GetCompanyInfo", new Hashtable() { });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetCompanyInfo fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public UserCompanyDto GetDefaultUserCompanyInfo(string userName)
        {
            try
            {
                var mapper = new DataMapper<UserCompanyDto>("GetUserCompanyInfo", new Hashtable() { { "UserName", userName } });

                var result = mapper.ObjectList();
                return result.Where(c => c.IsDefault == true).SingleOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetCompanyInfo fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public bool SetAsDefaultCompany(UserCompanyDto userCompany)
        {
            dataOperation.CommandText("auth_UserCompanySetDefault");
            dataOperation.AddParam("@LoginId", userCompany.UserName);
            dataOperation.AddParam("@CompanyId", userCompany.CompanyId);
            dataOperation.DoExecute();
            return true;
        }
    }
}
