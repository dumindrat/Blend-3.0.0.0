using Blend.BuisnessEntities.Purchase;
using Blend.DataModel.DataOperation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.UploadSale
{
    public class SaleUploadService : BaseManagement<UploadLog>, ISaleUploadService
    {
        public bool Log(BuisnessEntities.Purchase.UploadLog obj)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_SaleUploadLogInsert";
                cmd.Parameters.AddWithValue("@FileName", obj.FileName);
                cmd.Parameters.AddWithValue("@SaleId", obj.SaleId);
                cmd.Parameters.AddWithValue("@ErrorRecords", obj.ErrorRecords);
                cmd.Parameters.AddWithValue("@TotalRecords", obj.TotalRecords);
                cmd.Parameters.AddWithValue("@BrokerId", obj.BrokerId);
                cmd.Parameters.AddWithValue("@SaleCode", obj.SaleCode);
                cmd.Parameters.AddWithValue("@SaleType", obj.SaleType);
                cmd.Parameters.AddWithValue("@UploadData", obj.UploadData);
                cmd.Parameters.AddWithValue("@CreateUser", obj.User);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Ërror");
                return false;
            }
            finally
            {
                connection.Close();
                
            }
            return true;
        }
    }
}
