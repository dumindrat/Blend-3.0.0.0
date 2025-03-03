using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessEntities.BlendCreation;
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

namespace Blend.BuisnessService.BlendClose
{
    public class BlendCloseService : BaseManagement<BlendInfo>, IBlendCloseService
    {
        public BlendCloseService()
        {
        }

        public IList<BuisnessEntities.BlendCreation.BlendInfo> GetBlendList(string serchBlendNo)
        {
            try
            {
                var searchBy = serchBlendNo == "all" ? "" : serchBlendNo;
                var mapper = new DataMapper<BlendInfo>("GetBlendsForClose", new Hashtable() { { "BlendNo", searchBy } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendList fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public bool BlendClose(BlendCloseDto obj)
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
                cmd.CommandText = "trn_BlendCategorizeUpdateByBlendClose";
                //All validation is in stored proc.
                cmd.Parameters.AddWithValue("@BBlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@PhysicalQty", obj.PhysicalQty);
                cmd.Parameters.AddWithValue("@BlendBal", obj.BB);
                cmd.Parameters.AddWithValue("@BlendGain", obj.BG);
                cmd.Parameters.AddWithValue("@BlendLoss", obj.BL);
                cmd.Parameters.AddWithValue("@StdBBal", obj.STDBB);
                cmd.Parameters.AddWithValue("@StdBGain", obj.STDBG);
                cmd.Parameters.AddWithValue("@SftTea", obj.SiftedQty);
                cmd.Parameters.AddWithValue("@SftBBal", obj.SFTBB);
                cmd.Parameters.AddWithValue("@SftBGain", obj.SFTBG);
                cmd.Parameters.AddWithValue("@CloseDate", obj.CloseDate);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@RowVer", obj.RowVersion);
                cmd.ExecuteNonQuery();

                //Step 1:

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                //cmd.Parameters.AddWithValue("@stkQty", obj.BB > obj.ExportQty ? -obj.ExportQty : -obj.BB);//Blend balance reducing export Qty and BL
                cmd.Parameters.AddWithValue("@stkQty", -obj.LineQty);
                cmd.ExecuteNonQuery();

                //Step 1.1 
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockInsert";
                cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " SHIPMENT STOCKS");
                cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                cmd.Parameters.AddWithValue("@stkItemType", "STOCK");
                cmd.Parameters.AddWithValue("@stkPack", 0);
                cmd.Parameters.AddWithValue("@stkQty", obj.ExportQty); //Create Export Qty as Stock to Shipment Disposal
                cmd.ExecuteNonQuery();

                if (obj.STDBB > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                    cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkType", "BB");
                    cmd.Parameters.AddWithValue("@stkPacks", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.STDBB);//Blend balance if remain bb after exported
                    cmd.ExecuteNonQuery();
                }

                //Step 2:
                //if (obj.SFTBB > 0)
                //{
                //    cmd = new SqlCommand();
                //    cmd.Transaction = trans;
                //    cmd.Connection = connection;
                //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //    cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                //    cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                //    cmd.Parameters.AddWithValue("@stkType", "BB");
                //    cmd.Parameters.AddWithValue("@stkPacks", 0);
                //    cmd.Parameters.AddWithValue("@stkQty", -obj.SFTBB);//Blend balance reducing sifted balance Qty
                //    cmd.ExecuteNonQuery();
                //}
                //Step 3:
                //if (obj.BL > 0)
                //{
                //    cmd = new SqlCommand();
                //    cmd.Transaction = trans;
                //    cmd.Connection = connection;
                //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //    cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                //    cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                //    cmd.Parameters.AddWithValue("@stkType", "BB");
                //    cmd.Parameters.AddWithValue("@stkPacks", 0);
                //    cmd.Parameters.AddWithValue("@stkQty", -obj.BL);//Blend Loss reducing export 
                //    cmd.ExecuteNonQuery();
                //}
                //Standard Gain
                if (obj.STDBG > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " Blend Gain");
                    cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                    cmd.Parameters.AddWithValue("@stkItemType", "STDBG");
                    cmd.Parameters.AddWithValue("@stkPack", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.STDBG);
                    cmd.ExecuteNonQuery();
                }

                //Sifted Balance
                if (obj.SFTBB > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " Sifted Balance");
                    cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                    cmd.Parameters.AddWithValue("@stkItemType", "SFTBB");
                    cmd.Parameters.AddWithValue("@stkPack", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.SFTBB);
                    cmd.ExecuteNonQuery();
                }

                //Sifted Gain
                if (obj.SFTBG > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("@stItemId", obj.BlendId);
                    cmd.Parameters.AddWithValue("@stkItemName", obj.BlendNo + " Sifted Gain");
                    cmd.Parameters.AddWithValue("@stkName", obj.BlendNo);
                    cmd.Parameters.AddWithValue("@stkItemType", "SFTBG");
                    cmd.Parameters.AddWithValue("@stkPack", 0);
                    cmd.Parameters.AddWithValue("@stkQty", obj.SFTBG);
                    cmd.ExecuteNonQuery();
                }

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendHeaderUpdateCloseStatus";
                cmd.Parameters.AddWithValue("@blendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@closeStatus", 1);
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


        public bool BlendCloseCancelRequest(BlendCloseReversDto obj)
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
                cmd.CommandText = "trn_BlendCategorizeCancelRequestInsert";
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@RequestBy", obj.User);
                cmd.Parameters.AddWithValue("@Reason", obj.Reason);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeUpdateByCancelRequest";
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@RowVersion", obj.RowVersion);
                cmd.ExecuteNonQuery();

                trans.Commit();

                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                cmd.Dispose();
            }
        }


        public IList<BlendCloseDto> GetBlendCloseList(string serchBlendNo)
        {
            try
            {
                var searchBy = serchBlendNo == "all" ? "" : serchBlendNo;
                var mapper = new DataMapper<BlendCloseDto>("GetClsedBlendsForReverse", new Hashtable() { { "BlendNo", searchBy } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendList fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public IList<BlendCloseCancelRequest> GetBlendCancelRequest(string searchBlendNo)
        {
            try
            {
                var searchBy = searchBlendNo == "all" ? "" : searchBlendNo;
                var mapper = new DataMapper<BlendCloseCancelRequest>("GetBlendCancelRequest", new Hashtable() { { "BlendNo", searchBy } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendList fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public bool ApproveBlendCloseCancel(BlendCloseReversDto obj)
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
                cmd.CommandText = "trn_BlendCategorizeLogGenerate";
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeCancelRequestApprove";
                //All validation is in stored proc.
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@RowVersion", obj.RowVersion);
                cmd.ExecuteNonQuery();


                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockDeleteByCancelBlendClose";
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByCancelBlendClose";
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeUpdateByCancelRequestApprove";
                cmd.Parameters.AddWithValue("@StockItemId", obj.BlendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendHeaderUpdateCloseStatus";
                cmd.Parameters.AddWithValue("@blendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@closeStatus", 0);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[trn_TeaShipmentDelete]";
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
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


        public BlendCloseDto GetBlendCloseInfoByBlendid(int blendId)
        {
            try
            {
                var mapper = new DataMapper<BlendCloseDto>("GetBlendCloseInfo", new Hashtable() { { "BlendId", blendId } });

                var result = mapper.Object();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendCloseInfoByBlendid fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }
    }
}
