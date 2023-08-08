using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBatisNet.DataMapper;

namespace Blend.Data.User
{
    public class UserManagement //: BaseManagement<SystemUser>
    {
        //public void CreateUser(SystemUser obj)
        //{
        //    this.BaseRepo.Create(EnrichData(obj));
        //}

        //public SystemUser GetUserByLoginId(string loginId)
        //{
        //    var retObj = this.Mapper.QueryForObject<SystemUser>("GetUser", new SystemUser() { LoginId = loginId });
        //    return retObj;
        //}

        //private SystemUser EnrichData(SystemUser obj)
        //{
        //    obj.LoginId = EncryptData.Encrypt(obj.LoginId);
        //    obj.Password = EncryptData.Encrypt(obj.Password);
        //    obj.OperationCommand = "InsertUser";
        //    return obj;
        //}

        //public override SystemUser Create(SystemUser obj)
        //{
        //    throw new NotImplementedException();
        //}

        //public override SystemUser Update(SystemUser obj)
        //{
        //    throw new NotImplementedException();
        //}

        //public override SystemUser GetDataById(int key)
        //{
        //    throw new NotImplementedException();
        //}

        //public override SystemUser Delete(SystemUser obj)
        //{
        //    throw new NotImplementedException();
        //}

        //public override IList<SystemUser> GetDataList(long key)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
