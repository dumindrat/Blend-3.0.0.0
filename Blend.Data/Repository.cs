using IBatisNet.DataMapper;

namespace Blend.Data
{
    public class Repository<T> : IOperation<T>
    {
        readonly ISqlMapper _mapper;
        public Repository()
        {
            _mapper = Mapper.Instance();
        }

        public void Create(T obj)
        {
            //PropertyInfo propInfo = obj.GetType().GetProperty("OperationCommand");
            //object value = propInfo.GetValue(obj, null);
            _mapper.Insert(OperationCmmand.ExtractCommand(obj), obj);
        }

        public T Retrieve(T obj)
        {
            var retObj = _mapper.QueryForObject<T>(OperationCmmand.ExtractCommand(obj), obj);
            return retObj;
        }

        public void Update(T obj)
        {
            _mapper.Update(OperationCmmand.ExtractCommand(obj), obj);
        }

        public void Delete(T obj)
        {
            _mapper.Delete(OperationCmmand.ExtractCommand(obj), obj);
        }


        public T GetDataById(T obj)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IList<T> GetDataList(long key)
        {
            throw new System.NotImplementedException();
        }
    }

    public static class OperationCmmand
    {
        public static string ExtractCommand(object obj)
        {
            var propInfo = obj.GetType().GetProperty("OperationCommand");
            var value = propInfo.GetValue(obj, null);
            return value.ToString();
        }
    }
}
