using Blend.BuisnessEntities.Packing.ItemIssue;
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

namespace Blend.BuisnessService.Packing.ItemIssue
{
    public class ItemIssueService : IItemIssueService
    {
        private IDocumentService _documentService;
        public ItemIssueService(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        public IEnumerable<BuisnessEntities.Packing.ItemIssue.ItemIssueNoteDto> GetItemIssueNoteList(string id, int companyId)
        {
            var searchBy = id == "all" ? "" : id;
            var mapper = new DataMapper<ItemIssueNoteDto>("GetIn", new Hashtable() { { "IssueNoteNo", searchBy }, { "CompanyId", companyId } });
            var result = mapper.ObjectList();
            return result;
        }


        public bool Create(ItemIssueNoteDto obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            var poDocType = 5;
            try
            {
                var blendNo = new DocumentNo();
                if (obj.IsAutoDocNo)
                    obj.IssueNoteNo = _documentService.GetDocumentNo(poDocType, obj.CompanyId).FormattedDocNo;
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemIssueNoteInsert";
                cmd.Parameters.AddWithValue("@INNumber", obj.IssueNoteNo);
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@CompanyId", obj.CompanyId);

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
                    cmd.Parameters.AddWithValue("@CompanyId", obj.CompanyId);
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

        public bool Update(ItemIssueNoteDto obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemIssueNoteUpdate";
                cmd.Parameters.AddWithValue("@Id", obj.Id);
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@User", obj.User);

                cmd.ExecuteNonQuery();
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

        public bool Delete(int id)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_ItemIssueNoteDelete";
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();

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

        public bool AddItem(IssueNoteItem item)
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
                cmd.CommandText = "pack_ItemIssueNoteDetailInsert";
                cmd.Parameters.AddWithValue("@ItemIssueNoteId", item.IssueNoteId);
                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                cmd.Parameters.AddWithValue("@Qty", item.Qty);
                cmd.Parameters.AddWithValue("@User", item.User);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[pack_ItemStockUpdate]";
                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@Qty", -item.Qty);
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

        public bool RemoveItem(IssueNoteItem item)
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
                cmd.CommandText = "pack_ItemIssueNoteDetailDelete";
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[pack_ItemStockUpdate]";
                cmd.Parameters.AddWithValue("@StockId", item.StockId);
                cmd.Parameters.AddWithValue("@Qty", item.Qty);
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

        public IEnumerable<StockItem> GetStockItemList(string id)
        {
            var searchBy = id == "all" ? "" : id;
            var mapper = new DataMapper<StockItem>("GetStockItems", new Hashtable() { { "ItemName", searchBy } });
            var result = mapper.ObjectList();
            return result;
        }


        public IEnumerable<IssueNoteItem> GetIssueNoteItemById(int id)
        {
            var mapper = new DataMapper<IssueNoteItem>("GetInItems", new Hashtable() { { "Id", id } });
            var result = mapper.ObjectList();
            return result;
        }
    }
}
