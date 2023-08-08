using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.DataModel.DataOperation
{
    public static class MSSqlDataService 
    {
        //private SqlTransaction transaction = null;
        public static SqlCommand MsSQLCmd
        {
            get
            {
                var mySQLcmd = new SqlCommand();
                mySQLcmd.CommandType = System.Data.CommandType.StoredProcedure;
                return mySQLcmd;
            }
        }

        public static SqlConnection MsSQLConn
        {
            get
            {
                return new SqlConnection(ConfigurationManager.ConnectionStrings["msSQLConnection"].ConnectionString);
            }
        }

        //public static SqlTransaction MsSqlTransaction
        //{
        //    get
        //    {
        //        return transaction;
        //    }
        //}

       
    }
}
