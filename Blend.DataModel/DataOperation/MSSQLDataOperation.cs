using Blend.BuisnessEntities;
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
    public class MSSQLDataOperation : IDataOperation
    {
        protected readonly SqlConnection msSQLConn;
        protected readonly SqlCommand msSQLcmd;
        public string commandText { get; set; }
        protected SqlTransaction trans = null;
        protected BaseLogWriter logger = new BaseLogWriter();
        public MSSQLDataOperation()
        {
            try
            {
                msSQLConn = new SqlConnection(ConfigurationManager.ConnectionStrings["msSQLConnection"].ConnectionString);
                //logger.LogWrite("Accessing " + msSQLConn.DataSource + " and " + msSQLConn.Database);
                msSQLcmd = new SqlCommand();
                msSQLcmd.CommandType = System.Data.CommandType.StoredProcedure;
                //msSQLConn.Open();
                //logger.LogWrite("Connection open success for Connection :" + msSQLConn.ConnectionString);
                ///msSQLcmd.Connection = msSQLConn;
            }
            catch (Exception ex)
            {
                logger.LogWrite("Error in db connection operation " + ex.Message);
            }
        }

        public void DoExecute()
        {
            try
            {
                msSQLConn.Open();
                msSQLcmd.Connection = msSQLConn;
                msSQLcmd.ExecuteNonQuery();
                msSQLConn.Close();
                msSQLcmd.Dispose();
                msSQLConn.Dispose();
            }
            catch (SqlException ex)
            {
                
                throw ex;
            }
        }

        public void BeginTrans()
        {
            trans = msSQLConn.BeginTransaction();
            msSQLcmd.Transaction = trans;

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
            msSQLcmd.Parameters.AddWithValue(paramName, value);
            msSQLcmd.Parameters[paramName].Direction = System.Data.ParameterDirection.Input;
        }

        public void CommandText(string commandText)
        {
            msSQLcmd.CommandText = commandText;
        }


        public void ConnectionClose()
        {
            msSQLConn.Close();
            msSQLConn.Dispose();
        }
    }
}
