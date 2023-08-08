using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using Blend.DataModel.DataOperation;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Adjustent
{
    public class AdjustmentService : BaseManagement<StraightLineStockItem>, IAdjustmentService
    {
        private int reject = 1;
        private int shortline = 2;
        public IEnumerable<BuisnessEntities.BlendCreation.StraightLineStockItem> GetStraightLines(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var mapper = new DataMapper<StraightLineStockItem>("GetLinesForReject", new Hashtable() { { "BoxNo", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool AddToReject(BuisnessEntities.Adjustment.AdjutLineDto obj)
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
                cmd.CommandText = "trn_LineAdjustmentInsert";
                cmd.Parameters.AddWithValue("purchaseId", obj.PurchaseId);
                cmd.Parameters.AddWithValue("stockId", obj.StockId);
                cmd.Parameters.AddWithValue("packs", obj.AdjustBags);
                cmd.Parameters.AddWithValue("qty", obj.AdjustQty);
                cmd.Parameters.AddWithValue("adjustmentType", reject);
                cmd.Parameters.AddWithValue("reason", obj.Reason);
                cmd.Parameters.AddWithValue("user", obj.User);
                cmd.Parameters.AddWithValue("rowVersion", obj.RowVersion);

                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockRemove";
                cmd.Parameters.AddWithValue("stkId", obj.StockId);
                cmd.Parameters.AddWithValue("stkPacks", obj.AdjustBags);
                cmd.Parameters.AddWithValue("stkQty", obj.AdjustQty);
                cmd.ExecuteNonQuery();

                trans.Commit();
                cmd.Dispose();
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return true;
        }

        public bool AddToShort(BuisnessEntities.Adjustment.AdjutLineDto obj)
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
                cmd.CommandText = "trn_LineAdjustmentInsert";
                cmd.Parameters.AddWithValue("purchaseId", obj.PurchaseId);
                cmd.Parameters.AddWithValue("stockId", obj.StockId);
                cmd.Parameters.AddWithValue("packs", obj.AdjustBags);
                cmd.Parameters.AddWithValue("qty", obj.AdjustQty);
                cmd.Parameters.AddWithValue("adjustmentType", shortline);
                cmd.Parameters.AddWithValue("reason", obj.Reason);
                cmd.Parameters.AddWithValue("user", obj.User);
                cmd.Parameters.AddWithValue("rowVersion", obj.RowVersion);

                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockRemove";
                cmd.Parameters.AddWithValue("stkId", obj.StockId);
                cmd.Parameters.AddWithValue("stkPacks", obj.AdjustBags);
                cmd.Parameters.AddWithValue("stkQty", obj.AdjustQty);
                cmd.ExecuteNonQuery();

                trans.Commit();
                cmd.Dispose();
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return true;
        }

        public IEnumerable<AdjutedLineDto> GetAdjustedLineById(int Id)
        {
            var mapper = new DataMapper<AdjutedLineDto>("GetAdjustedLine", new Hashtable() { { "StockId", Id } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool CancelRequest(AdjutLineCancelRequest obj)
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
                cmd.CommandText = "trn_LineAdjustmentCancelRequestInsert";
                cmd.Parameters.AddWithValue("adjId", obj.Id);
                cmd.Parameters.AddWithValue("reason", obj.CancelReason);
                cmd.Parameters.AddWithValue("user", obj.User);
                cmd.Parameters.AddWithValue("rowVersion", obj.RowVersion);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_LineAdjustmentUpdate";
                cmd.Parameters.AddWithValue("adjustmentId", obj.Id);
                cmd.Parameters.AddWithValue("reason", obj.CancelReason);
                cmd.Parameters.AddWithValue("user", obj.User);
                cmd.ExecuteNonQuery();
                trans.Commit();
                cmd.Dispose();
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return true;
        }


        public IEnumerable<AdjutLineCancelRequest> GetAdjusteLineCancelRequests(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var mapper = new DataMapper<AdjutLineCancelRequest>("GetAdjustedCancelRequest", new Hashtable() { { "BoxNo", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool ApproveCancelRequest(AdjutLineCancelRequest obj)
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
                cmd.CommandText = "[trn_LineAdjustmentCancelApprove]";
                cmd.Parameters.AddWithValue("@id", obj.Id);
                cmd.Parameters.AddWithValue("@adjustmentId", obj.AdjustmentId);
                cmd.Parameters.AddWithValue("@user", obj.User);
                cmd.Parameters.AddWithValue("@rowVersion", obj.RowVersion);

                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdate";
                cmd.Parameters.AddWithValue("stkId", obj.StockId);
                cmd.Parameters.AddWithValue("stkPacks", obj.Packs);
                cmd.Parameters.AddWithValue("stkQty", obj.Qty);
                cmd.ExecuteNonQuery();

                trans.Commit();
                cmd.Dispose();
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return true;
        }


        public IEnumerable<AdjutedLineDto> GetAdjustedLineByBoxNo(string boxNo)
        {
            var mapper = new DataMapper<AdjutedLineDto>("GetAdjustedLineByBox", new Hashtable() { { "BoxNo", boxNo } });

            var result = mapper.ObjectList();
            return result;
        }


        public IEnumerable<PrintNoteDto> GetAdjustedLinesByBrokerAndType(int brokerId, int type, int status)
        {
            var mapper = new DataMapper<PrintNoteDto>("GetAdjustedLineByBrokerAndType", new Hashtable() { { "BrokerId", brokerId }, { "Type", type }, { "Status", status } });

            var result = mapper.ObjectList();
            return result;
        }


        public bool SettlePayment(int adjustmentId, string user)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            connection.Open();
            cmd.Connection = connection;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "trn_LineAdjustmentPaymentSettle";
            cmd.Parameters.AddWithValue("@Id", adjustmentId);
            cmd.Parameters.AddWithValue("@User", user);

            cmd.ExecuteNonQuery();
            return true;
        }
    }
}
