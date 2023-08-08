using Blend.BuisnessEntities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.DataModel.DataOperation
{
    public class MySQLDataOperation : IDataOperation
    {
        protected readonly MySqlConnection mySQLConn;
        protected readonly MySqlCommand mySQLcmd;
        public string commandText { get; set; }
        protected MySqlTransaction trans = null;
        protected BaseLogWriter logger = new BaseLogWriter();
        public MySQLDataOperation()
        {
            try
            {
                mySQLConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["mqSQLConnection"].ConnectionString);
                logger.LogWrite("Accessing " + mySQLConn.DataSource + " and " + mySQLConn.Database);
                mySQLcmd = new MySqlCommand();
                mySQLcmd.CommandType = System.Data.CommandType.StoredProcedure;
                mySQLConn.Open();
                logger.LogWrite("Connection open success for Connection :" + mySQLConn.ConnectionString);
                mySQLcmd.Connection = mySQLConn;
            }
            catch (Exception ex)
            {
                logger.LogWrite("Error in db connection operation " + ex.Message);
                throw ex;
            }
        }
        public void DoExecute()
        {
            mySQLcmd.ExecuteNonQuery();
            mySQLConn.Close();
            mySQLcmd.Dispose();
        }

        public void BeginTrans()
        {
            trans = mySQLConn.BeginTransaction();
            mySQLcmd.Transaction = trans;

        }
        public void CommitTrans()
        {
            trans.Commit();
        }
        public void RollBackTrans()
        {
            trans.Rollback();
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


        public void ConnectionClose()
        {
            throw new NotImplementedException();
        }
    }
}
