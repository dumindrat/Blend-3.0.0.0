using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IBatisNet.DataMapper;

namespace Blend.Data.Blend
{
    public class BlendManagement : BaseManagement<BlendInfo>
    {
        public void Create(BlendInfo obj)
        {
            try
            {
                Mapper.BeginTransaction();
                obj.OperationCommand = "InsertBlendHeader";
                BaseRepo.Create(obj);
                Mapper.CommitTransaction();
            }
            catch (System.Exception ex)
            {
                Mapper.RollBackTransaction();
                throw;
            }
        }

        public void Update(BlendInfo obj)
        {
            try
            {
                Mapper.BeginTransaction();
                obj.OperationCommand = "UpdateBlendHeader";
                BaseRepo.Update(obj);
                BaseRepo.Update(obj);
            }
            catch (System.Exception ex)
            {
                Mapper.RollBackTransaction();
                throw;
            }
        }

        public BlendInfo GetDataById(int key)
        {
            var blend = Mapper.QueryForObject<BlendInfo>("GetBlendHeaderByBlendId", new Hashtable { { "Id", key } });
            return blend;
        }

        public void Delete(BlendInfo obj)
        {
            try
            {
                Mapper.BeginTransaction();
                obj.OperationCommand = "DeleteBlendHeader";
                BaseRepo.Delete(obj);
                Mapper.CommitTransaction();
                return obj;
            }
            catch (System.Exception ex)
            {
                Mapper.RollBackTransaction();
                throw;
            }
        }

        public void AddLines(BlendDetail obj)
        {
            try
            {
                Mapper.BeginTransaction();
                Mapper.Insert("AddLines", obj);
                Mapper.CommitTransaction();
                return;
            }
            catch (System.Exception ex)
            {
                Mapper.RollBackTransaction();
                throw;
            }
        }

        public void RemoveLines(BlendDetail obj)
        {
            try
            {
                Mapper.BeginTransaction();
                Mapper.Delete("RemoveLines", obj);
                Mapper.CommitTransaction();
            }
            catch (System.Exception ex)
            {
                Mapper.RollBackTransaction();
                //Log
                throw;
            }
            return;
        }

        public List<BlendDetail> GetBlendDataByBlendId(int id)
        {
            var data = Mapper.QueryForList<BlendDetail>("GetBlendDataById", new Hashtable { { "Id", id } }).ToList();
            return data;
        }

        public override IList<BlendInfo> GetDataList(long key)
        {
            throw new System.NotImplementedException();
        }
    }
}
