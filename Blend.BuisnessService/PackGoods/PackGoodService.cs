using Blend.BuisnessService.Helper.Email;
using Blend.DataModel.DataOperation;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.PackGoods;

namespace Blend.BuisnessService.PackGoods
{
    public class PackGoodService : BaseManagement<PackGoodDto>, IPackGoodService
    {
        public PackGoodService()
        {
        }

        public bool ConvertPackGoods(PackGoodDto obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            try
            {
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[trn_PackGoodConvert]";
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@AvailbleStock", obj.BB);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", -obj.BB);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", obj.GoodTea);
                cmd.ExecuteNonQuery();

                if (obj.FlavouredTea > 0)
                {
                    //Step 1.1 
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " FLAVOURED");
                    cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                    cmd.Parameters.AddWithValue("@stkItemType", "BB");
                    cmd.Parameters.AddWithValue("@stkPack", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.FlavouredTea); 
                    cmd.ExecuteNonQuery();
                }
                //Step 2:
                if (obj.PackGoodTea > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " PACK GOODS");
                    cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                    cmd.Parameters.AddWithValue("@stkItemType", "BB");
                    cmd.Parameters.AddWithValue("@stkPack", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.PackGoodTea); 
                    cmd.ExecuteNonQuery();
                }


                trans.Commit();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return true;

        }

    }
}
