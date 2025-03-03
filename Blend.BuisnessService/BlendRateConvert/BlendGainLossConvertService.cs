using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Blend.DataModel.DataOperation;

namespace Blend.BuisnessService.BlendRateConvert
{
    public class BlendGainLossConvertService : BaseManagement<BlendGainLossRateConvert>, IBlendGainLossConvertService
    {

        public bool ConvertGain(BlendGainLossRateConvert obj)
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

                var gainRate = GetRate(obj.BlendId);
                cmd.CommandText = "trn_BlendCategorizeConvertRate";
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@ConvertedRate", gainRate);
                cmd.Parameters.AddWithValue("@RowVersion", obj.RowVersion);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.ExecuteNonQuery();
                if (obj.BG > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_BlendCategorizeConvertBG";
                    cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                    cmd.ExecuteNonQuery();
                }
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockAdjustemnt";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "STDBG");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", 0);//Remove BG
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockAdjustemnt";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", obj.BB + obj.BG);//Blend Gain add to BB 
                cmd.ExecuteNonQuery();
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

        public bool ConvertLoss(BlendGainLossRateConvert obj)
        {
            var lossRate = GetRate(obj.BlendId);
            dataOperation.CommandText("trn_BlendCategorizeConvertRate");
            dataOperation.AddParam("@BlendId", obj.BlendId);
            dataOperation.AddParam("@ConvertedRate", lossRate);
            dataOperation.AddParam("@RowVersion", obj.RowVersion);
            dataOperation.AddParam("@User", obj.User);
            dataOperation.DoExecute();
            return true;
        }

        public decimal GetRate(int blendId)
        {
            var mapper = new DataMapper<BlendDetail>("GetBlendDetailById", new Hashtable() { { "BlendId", blendId } });
            var details = mapper.ObjectList();
            var totvalue = details.Sum(c => c.Value);
            
            var closeMapper = new DataMapper<BlendCloseDto>("GetBlendCloseInfo", new Hashtable() { { "BlendId", blendId } });
            var result = closeMapper.Object();
            //var totvalue = result.ClosedRate * result.LineQty;

            var qty = result.BG > 0 ? result.BG : -result.BL;
            var gainRate = totvalue / (result.LineQty + qty);
            return Math.Round(gainRate, 2);
        }

    }
}
