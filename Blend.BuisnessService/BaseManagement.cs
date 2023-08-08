using System.Collections.Generic;
using IBatisNet.DataMapper;
using MySql.Data.MySqlClient;
using System.Configuration;
using Blend.DataModel.DataOperation;
using log4net;
using Blend.BuisnessEntities;

namespace Blend.BuisnessService
{
    public abstract class BaseManagement<T>
    {
        protected ISqlMapper Mapper;
        protected readonly MySqlConnection mySQLConn;
        protected readonly MySqlCommand mySQLcmd;
        protected IDataOperation dataOperation;
        protected MySqlDataService dataSvc;

        protected string db = "mssql";
        //protected ILog logger = log4net.LogManager.GetLogger("applog");
        protected BaseLogWriter logger = new BaseLogWriter();
        protected BaseManagement()
        {
            //BaseRepo = new Repository<T>();
            //Mapper = IBatisNet.DataMapper.Mapper.Instance();
            dataOperation = DataOperationFactory.GetDataOperation(db);
            //dataSvc = new MySqlDataService(); //DataOperationFactory.GetDataServices(db);
        }


    }
}
