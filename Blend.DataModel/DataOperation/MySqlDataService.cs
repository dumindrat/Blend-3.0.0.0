using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.DataModel.DataOperation
{
    public class MySqlDataService : IDataOperation
    {
        private MySqlTransaction transaction = null;
        public MySqlCommand MySQLCmd
        {
            get
            {
                var mySQLcmd = new MySqlCommand();
                mySQLcmd.CommandType = System.Data.CommandType.StoredProcedure;
                return mySQLcmd;
            }
        }

        public MySqlConnection MySQLConn
        {
            get
            {
                return new MySqlConnection(ConfigurationManager.ConnectionStrings["mqSQLConnection"].ConnectionString);
            }
        }

        public MySqlTransaction MySqlTransaction
        {
            get
            {
                return transaction;
            }
        }

        public void DoExecute()
        {
            throw new NotImplementedException();
        }

        public void AddParam(string paramName, object value)
        {
            throw new NotImplementedException();
        }

        public void BeginTrans()
        {
            throw new NotImplementedException();
        }

        public void CommitTrans()
        {
            throw new NotImplementedException();
        }

        public void RollBackTrans()
        {
            throw new NotImplementedException();
        }

        public void CommandText(string commandText)
        {
            throw new NotImplementedException();
        }


        public void ConnectionClose()
        {
            throw new NotImplementedException();
        }
    }
}
