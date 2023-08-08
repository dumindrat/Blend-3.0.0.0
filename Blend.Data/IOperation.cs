using System.Collections.Generic;
namespace Blend.Data
{
    public interface IOperation<T>
    {
        void Create(T obj);
        void Update(T obj);
        void Delete(T obj);
        T GetDataById(T obj);
        IList<T> GetDataList(long key);
    }
}
