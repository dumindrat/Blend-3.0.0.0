using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.DataModel.DataOperation
{
    public static class DataOperationFactory
    {
        public static IDataOperation GetDataOperation(string dbType)
        {
            if (dbType == "mysql")
            {
                return new MySQLDataOperation();
            }
            else
            {
                return new MSSQLDataOperation();
            }
        }

        //public static IDataOperation GetDataServices(string dbType)
        //{
        //    if (dbType == "mysql")
        //    {
        //        return new MySqlDataService();
        //    }
        //    else
        //    {
        //        return new MSSqlDataService();
        //    }
        //}
    }
}
