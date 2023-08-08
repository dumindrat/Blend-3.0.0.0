using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBatisNet.DataMapper;

namespace Blend.Data.Standard
{
    public class StandardManagement : BaseManagement<TeaStandard>, IOperation<TeaStandard>
    {


        public void Create(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardInsert");
            dataOperation.AddParam("StandardName", obj.StandardName);
            dataOperation.DoExecute();
        }

        public void Update(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardUpdate");
            dataOperation.AddParam("Id", obj.StandardId);
            dataOperation.AddParam("StandardName", obj.StandardName);
            dataOperation.DoExecute();
        }

        public TeaStandard GetDataById(int key)
        {
            var obj = new TeaStandard() { StandardId = key };
            obj.OperationCommand = "GetStandardById";
            return this.BaseRepo.Retrieve(obj);
        }

        public void Delete(TeaStandard obj)
        {
            dataOperation.CommandText("trn_StandardDelete");
            dataOperation.AddParam("Id", obj.StandardId);
            dataOperation.DoExecute();
        }

        public IList<TeaStandard> GetDataList(long key)
        {
            var result = Mapper.QueryForList<TeaStandard>("GetStandards", new Hashtable() { });
            return result;
        }


        public TeaStandard GetDataById(TeaStandard obj)
        {
            throw new NotImplementedException();
        }
    }
}
