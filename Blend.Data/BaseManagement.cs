using System.Collections.Generic;
using IBatisNet.DataMapper;
using MySql.Data.MySqlClient;
using System.Configuration;
using Blend.Data.DataOperation;

namespace Blend.Data
{
    public abstract class BaseManagement<T>
    {
        protected readonly ISqlMapper Mapper;
        protected readonly Repository<T> BaseRepo;
        protected readonly MySqlConnection mySQLConn;
        protected readonly MySqlCommand mySQLcmd;
        protected readonly IDataOperation dataOperation;

        protected BaseManagement()
        {
            BaseRepo = new Repository<T>();
            Mapper = IBatisNet.DataMapper.Mapper.Instance();
            dataOperation = DataOperationFactory.GetDataOperation("mysql");
        }


    }
}
