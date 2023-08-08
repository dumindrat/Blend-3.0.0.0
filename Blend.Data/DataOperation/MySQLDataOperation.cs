using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.Data.DataOperation
{
    public class MySQLDataOperation : IDataOperation
    {
        protected readonly MySqlConnection mySQLConn;
        protected readonly MySqlCommand mySQLcmd;
        public string commandText { get; set; }

        public MySQLDataOperation()
        {
            mySQLConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["mqSQLConnection"].ConnectionString);
            mySQLcmd = new MySqlCommand();
            mySQLcmd.CommandType = System.Data.CommandType.StoredProcedure;
            mySQLConn.Open();
            mySQLcmd.Connection = mySQLConn;
        }
        public void DoExecute()
        {
            mySQLcmd.ExecuteNonQuery();
            mySQLConn.Close();
            mySQLcmd.Dispose();
        }

        public void AddParam(string paramName, object value)
        {
            mySQLcmd.Parameters.AddWithValue(paramName, value);
            mySQLcmd.Parameters[paramName].Direction = System.Data.ParameterDirection.Input;
        }

        public void CommandText(string commandText)
        {
            mySQLcmd.CommandText = commandText;
        }
    }
}
