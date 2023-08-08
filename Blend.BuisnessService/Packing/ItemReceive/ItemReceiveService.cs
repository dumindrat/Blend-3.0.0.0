using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.TransDocument;
using Blend.BuisnessService.Document;
using Blend.DataModel.DataOperation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.ItemReceive
{
    public class ItemReceiveService : IItemReceiveService
    {
        private IDocumentService _documentService;
        public ItemReceiveService(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IEnumerable<BuisnessEntities.Packing.ItemReceive.ItemReceiveNoteDto> GetItemReceiveNoteList(string id)
        {
            var searchBy = id == "all" ? "" : id;
            var mapper = new DataMapper<ItemReceiveNoteDto>("GetRn", new Hashtable() { { "ReceiveNoteNo", searchBy } });
            var result = mapper.ObjectList();
            return result;
        }
        
        public bool Create(BuisnessEntities.Packing.ItemReceive.ItemReceiveNoteDto receivenote)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            var poDocType = 4;
            var lastIncerementId = 0;

            try
            {
                var blendNo = new DocumentNo();
                if (receivenote.IsAutoDocNo)
                    receivenote.ReceiveNoteNo = _documentService.GetDocumentNo(poDocType).FormattedDocNo;
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReceivedNoteInsert";
                cmd.Parameters.AddWithValue("@ReceiveNoteNo", receivenote.ReceiveNoteNo);
                cmd.Parameters.AddWithValue("@PoId", receivenote.PoId);
                cmd.Parameters.AddWithValue("@User", receivenote.User);
                cmd.Parameters.AddWithValue("@ReceivedNoteId", lastIncerementId);
                cmd.Parameters["@ReceivedNoteId"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                var lastIncrementedId = (Int32)cmd.Parameters["@ReceivedNoteId"].Value;

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemReceivedNoteDetailInsert";
                cmd.Parameters.AddWithValue("@ReceiveNoteId", lastIncrementedId);
                cmd.Parameters.AddWithValue("@PoId", receivenote.PoId);
                cmd.ExecuteNonQuery();

                if (receivenote.IsAutoDocNo)
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

        public bool UpdateItemQty(UpdateReceivedItemDto receivenoteitem)
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
                cmd.CommandText = "pack_ItemReceivedUpdate";
                cmd.Parameters.AddWithValue("@ReceivedNoteId", receivenoteitem.ReceivedNoteId);
                cmd.Parameters.AddWithValue("@ItemId", receivenoteitem.ItemId);
                cmd.Parameters.AddWithValue("@ReceivedQty", receivenoteitem.ReceivedQty);
                cmd.Parameters.AddWithValue("@ReceivedPrice", receivenoteitem.ReceivedPrice);
                cmd.Parameters.AddWithValue("@User", receivenoteitem.User);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[pack_ItemStockUpdateByEditItem]";
                cmd.Parameters.AddWithValue("@StockItemId", receivenoteitem.Id);
                cmd.Parameters.AddWithValue("@ItemMasterId", receivenoteitem.ItemId);
                cmd.Parameters.AddWithValue("@Qty", receivenoteitem.ReceivedQty);
                cmd.Parameters.AddWithValue("@ReceivedDate", receivenoteitem.ReceivedDate);
                cmd.Parameters.AddWithValue("@StoreLocation", receivenoteitem.StoreLocation == null ? "" : receivenoteitem.StoreLocation);
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

        public bool CreateItemQty(UpdateReceivedItemDto receivenoteitem)
        {
            throw new NotImplementedException();
        }
        
        public IEnumerable<AvailablePoDto> GetPoList()
        {
            var mapper = new DataMapper<AvailablePoDto>("GetRnPo", new Hashtable() { });
            var result = mapper.ObjectList();
            return result;
        }


        public IEnumerable<ReceiveNoteItems> GetReceiveNoteItems(int id)
        {
            var mapper = new DataMapper<ReceiveNoteItems>("ReceiveNoteItems", new Hashtable() { { "ReceivedNoteId", id } });
            var result = mapper.ObjectList();
            return result;
        }


        public bool AddItemQty(UpdateReceivedItemDto receivenoteitem)
        {
            throw new NotImplementedException();
        }


        public ItemReceiveNoteDto GetReceiveNoteById(int id)
        {
            var mapper = new DataMapper<ItemReceiveNoteDto>("GetRnById", new Hashtable() { { "Id", id } });
            var result = mapper.Object();
            return result;
        }
    }
}
