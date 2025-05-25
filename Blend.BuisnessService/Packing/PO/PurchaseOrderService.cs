using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessEntities.TransDocument;
using Blend.BuisnessService.Document;
using Blend.DataModel.DataOperation;
using Blend.BuisnessService.Helper.Email;

namespace Blend.BuisnessService.Packing.PO
{
    public class PurchaseOrderService : BaseManagement<PurchaseOrderDto>, IPurchaseOrderService
    {
        private IDocumentService _documentService;
        public IEmailService _emailService;
        public PurchaseOrderService(IDocumentService documentService, IEmailService emailService)
        {
            _documentService = documentService;
            _emailService = emailService;
        }

        public IEnumerable<BuisnessEntities.Packing.PO.PurchaseOrderDto> GetPOList(string id, int CompanyId)
        {
            var searchBy = id == "all" ? "" : id;

            var mapper = new DataMapper<PurchaseOrderDto>("GetPos", new Hashtable() { { "PONumber", searchBy }, { "CompanyId", CompanyId } });
            var result = mapper.ObjectList();
            return result;
        }

        public bool CreatePO(PurchaseOrderInfo po)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            var poDocType = 3;
            try
            {
                int? contract;
                contract = po.ContractId;
                if (po.ContractId == 0)
                {
                    contract = null;
                }
                var blendNo = new DocumentNo();
                if (po.IsAutoDocNo)
                    po.PONumber = _documentService.GetDocumentNo(poDocType, po.CompanyId).FormattedDocNo;
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_POInsert";
                cmd.Parameters.AddWithValue("@PONumber", po.PONumber);
                cmd.Parameters.AddWithValue("@SupplierId", po.SupplierId);
                cmd.Parameters.AddWithValue("@ContractNo", contract);
                cmd.Parameters.AddWithValue("@DeliveryDate", po.DeliveryDate);
                cmd.Parameters.AddWithValue("@Instructions", po.Instructions);
                cmd.Parameters.AddWithValue("@User", po.User);
                cmd.Parameters.AddWithValue("@CompanyId", po.CompanyId);
                cmd.ExecuteNonQuery();

                if (po.IsAutoDocNo)
                {
                    //Increase DocNo
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_DocumentNoGenerate";
                    cmd.Parameters.AddWithValue("@DocTypes", poDocType);
                    cmd.Parameters.AddWithValue("@CompanyId", po.CompanyId);
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

        public bool UpdatePO(PurchaseOrderInfo po)
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
                cmd.CommandText = "pack_POUpdate";
                cmd.Parameters.AddWithValue("@Id", po.Id);
                cmd.Parameters.AddWithValue("@SupplierId", po.SupplierId);
                cmd.Parameters.AddWithValue("@ContractId", po.ContractId);
                cmd.Parameters.AddWithValue("@DeliveryDate", po.DeliveryDate);
                cmd.Parameters.AddWithValue("@Instructions", po.Instructions);
                cmd.Parameters.AddWithValue("@User", po.User);
                cmd.Parameters.AddWithValue("@RowVersion", po.RowVersion);
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

        public bool DeletePO(int id)
        {
            dataOperation.CommandText("pack_PODelete");
            dataOperation.AddParam("@Id", id);
            dataOperation.DoExecute();
            return true;
        }

        public bool AddItem(PurchaseOrderItemDto item)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_POItemInsert";
                cmd.Parameters.AddWithValue("@PoId", item.PoId);
                cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                cmd.Parameters.AddWithValue("@Qty", item.Qty);
                cmd.Parameters.AddWithValue("@Price", item.Price);
                cmd.Parameters.AddWithValue("@Comment", item.Comment);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                logger.LogWrite("Error in adding item to PO " + item.PoId + ": " + ex.Message);
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

        public bool RemoveItem(PurchaseOrderItemDto item)
        {
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "pack_POItemDelete";
                cmd.Parameters.AddWithValue("@PoId", item.PoId);
                cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                cmd.ExecuteNonQuery();
                logger.LogWrite("Successfully Deleted item: " + item.ItemCode + " from PO: " + item.PoId);
            }

            catch (Exception ex)
            {
                logger.LogWrite("Error in deleting item : " + item.ItemCode + " from PO " + item.PoId + ": " + ex.Message);
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

        public IEnumerable<PurchaseOrderItemDto> GetPOItemList(int id)
        {
            var mapper = new DataMapper<PurchaseOrderItemDto>("GetPOItems", new Hashtable() { { "PoId", id } });
            var result = mapper.ObjectList();
            return result;
        }

        public IEnumerable<PurchaseOrderItemDto> GetItems(int id)
        {
            var mapper = new DataMapper<PurchaseOrderItemDto>("GetPOItems", new Hashtable() { { "PoId", id } });
            var result = mapper.ObjectList();
            return result;
        }

        public PurchaseOrderDto GetPOById(int id)
        {
            var mapper = new DataMapper<PurchaseOrderDto>("GetPoById", new Hashtable() { { "Id", id } });
            var result = mapper.Object();
            return result;
        }
    }
}
