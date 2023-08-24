using Blend.BuisnessEntities.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TeaStandards
{
    public class TeaStandardService : BaseManagement<TeaStandard>, ITeaStandardService
    {
        public TeaStandard GetTeaStandardById(int id)
        {
            throw new NotImplementedException();
        }

        public IList<TeaStandard> GetAllTeaStandard(string searchstr, int companyId)
        {
            var searchBy = searchstr == "all" ? "" : searchstr;
            var mapper = new DataMapper<TeaStandard>("GetStandards", new Hashtable() { { "StandardName", searchBy }, { "CompanyId", companyId } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool CreateStandard(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardInsert");
            dataOperation.AddParam("StandardName", obj.StandardName);
            dataOperation.AddParam("User", obj.User);
            dataOperation.AddParam("CompanyId", obj.CompanyId);
            dataOperation.DoExecute();
            return true;
        }

        public bool UpdateStandard(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardUpdate");
            dataOperation.AddParam("Id", obj.StandardId);
            dataOperation.AddParam("StandardName", obj.StandardName);
            dataOperation.AddParam("User", obj.User);
            dataOperation.AddParam("RowVersion", obj.RowVersion);
            dataOperation.DoExecute();
            return true;
        }

        public bool Deletetandard(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardDelete");
            dataOperation.AddParam("Id", obj.StandardId);
            dataOperation.DoExecute();
            return true;
        }
    }
}
