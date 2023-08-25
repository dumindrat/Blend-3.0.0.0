using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.ItemReturn;
using Blend.BuisnessEntities.TransDocument;
using Blend.BuisnessService.Document;
using Blend.DataModel.DataOperation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.ItemReturn
{
    public class ItemReturnService : IItemRetunService
    {
        private IDocumentService _documentService;
        public ItemReturnService(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        public IEnumerable<BuisnessEntities.Packing.ItemReturn.ItemReturnNoteDto> GetItemReturNoteList(string id)
        {
            var searchBy = id == "all" ? "" : id;
            var mapper = new DataMapper<ItemReturnNoteDto>("GetReturnNotes", new Hashtable() { { "ReturnNoteNo", searchBy } });
            var result = mapper.ObjectList();
            return result;
        }

        public IEnumerable<BuisnessEntities.Packing.ItemReceive.ItemReceiveNoteDto> GetReceiveNoteList()
        {
            var mapper = new DataMapper<ItemReceiveNoteDto>("GetReceiveNoteList", new Hashtable() { });
            var result = mapper.ObjectList();
            return result;
        }

        public bool Create(ItemReturnNoteDto obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            var poDocType = 6;
            try
            {
                var blendNo = new DocumentNo();
                if (obj.IsAutoDocNo)
                    obj.ReturnNoteNo = _documentService.GetDocumentNo(poDocType, obj.CompanyId).FormattedDocNo;
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReturnNoteInsert";
                cmd.Parameters.AddWithValue("@ReturnNoteNo", obj.ReturnNoteNo);
                cmd.Parameters.AddWithValue("@ReceiveNoteId", obj.ReceiveNoteId);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@Note", obj.Comment == null ? "" : obj.Comment);

                cmd.ExecuteNonQuery();

                if (obj.IsAutoDocNo)
                {
                    //Increase DocNo
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_DocumentNoGenerate";
                    cmd.Parameters.AddWithValue("@DocTypes", poDocType);
                    cmd.ExecuteNonQuery();

                }
                trans.Commit();
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
            return true;
        }

        public bool Update(ItemReturnNoteDto obj)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReturnNoteUpdate";
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@Comment", obj.Comment == null ? "" : obj.Comment);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                cmd.Dispose();
            }
            return true;
        }

        public bool Delete(int id)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReturnNoteDelete";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                cmd.Dispose();
            }
            return true;
        }

        public IEnumerable<ReturnNoteItems> GetReturnNoteItemById(int id)
        {
            var mapper = new DataMapper<ReturnNoteItems>("GetReturnNoteItems", new Hashtable() { { "ReturnNoteId", id } });
            var result = mapper.ObjectList();
            return result;
        }

        public IEnumerable<ReceiveNoteStockItems> GetReceiveNoteItemsforReturn(int id)
        {
            var mapper = new DataMapper<ReceiveNoteStockItems>("GetReceiveItemsForReturnNotes", new Hashtable() { { "ReceivedNoteId", id } });
            var result = mapper.ObjectList();
            return result;
        }

        public bool AddItem(ReturnNoteItems item)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            SqlTransaction trans = null;
            try
            {
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReturnNoteDetailInsert";
                cmd.Parameters.AddWithValue("@ReturnNoteId", item.ReturnNoteId);
                cmd.Parameters.AddWithValue("@ReceivedId", item.Id);
                cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                cmd.Parameters.AddWithValue("@Qty", item.ReturnQty);
                cmd.Parameters.AddWithValue("@Reason", item.Reason);
                cmd.Parameters.AddWithValue("@User", item.User);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[pack_ItemStockUpdate]";
                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@Qty", -item.ReturnQty);
                cmd.ExecuteNonQuery();

                trans.Commit();
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
            return true;
        }

        public bool RemoveItem(ReturnNoteItems item)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            SqlTransaction trans = null;
            try
            {
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReturnNoteDetailDelete";
                cmd.Parameters.AddWithValue("@ReturnNoteId", item.ReturnNoteId);
                cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[pack_ItemStockUpdate]";
                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@Qty", item.ReturnQty);
                cmd.ExecuteNonQuery();

                trans.Commit();
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
            return true;
        }


        public ItemReturnNoteDto GetReturNoteById(int id)
        {
            var mapper = new DataMapper<ItemReturnNoteDto>("GetReturnNoteById", new Hashtable() { { "Id", id } });
            var result = mapper.Object();
            return result;
        }
    }
}
