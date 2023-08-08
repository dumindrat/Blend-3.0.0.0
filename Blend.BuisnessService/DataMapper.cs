using IBatisNet.Common;
using IBatisNet.DataMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService
{
    public class DataMapper<T>
    {
        private ISqlMapper _mapper;
        private string _statement;
        private object _param;

        public DataMapper(string statement, object param)
        {
            _mapper = IBatisNet.DataMapper.Mapper.Instance(); ;
            _statement = statement;
            _param = param;
        }
        public IList<T> ObjectList()
        {
            using (IDalSession session = _mapper.OpenConnection())
            {
                return _mapper.QueryForList<T>(_statement, _param);
            }
        }

        public T Object()
        {
            using (IDalSession session = _mapper.OpenConnection())
            {
                return _mapper.QueryForObject<T>(_statement, _param);
            }
        }
    }
}
