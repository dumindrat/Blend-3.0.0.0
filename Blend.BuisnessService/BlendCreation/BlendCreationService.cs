using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.TransDocument;
using Blend.BuisnessService.Document;
using Blend.DataModel.DataOperation;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.BlendCreation
{
    public class BlendCreationService : BaseManagement<BlendInfo>, IBlendCreationService
    {
        private IDocumentService _documentService;
        private int blend = 2;

        public BlendCreationService(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        public IList<BlendInfo> GetBlendList(string serchBlendNo)
        {
            try
            {
                var searchBy = serchBlendNo == "all" ? "" : serchBlendNo;
                var mapper = new DataMapper<BlendInfo>("GetBlends", new Hashtable() { { "BlendNo", searchBy } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendList fails. Error " + ex.Message);
                throw ex;
            }

        }


        public BlendDetailDto GetBlendById(long id)
        {
            try
            {
                var mapper = new DataMapper<BlendDetail>("GetBlendDetailById", new Hashtable() { { "BlendId", id } });
                //var details = Mapper.QueryForList<BlendDetail>("GetBlendDetailById", new Hashtable() { { "BlendId", id } });
                var details = mapper.ObjectList();
                var result = new BlendDetailDto { BlendItem = new List<BlendDetail>() };
                result.BlendItem = details;
                var qty = details.Sum(c => c.Qty);
                var totvalue = details.Sum(c => c.Value);
                var rate = (totvalue == 0 || qty == 0) ? 0 : Decimal.Round((totvalue / qty), 2);
                result.DisplayQty = qty.ToString();
                result.DisplayRate = rate.ToString();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendById fails. Error " + ex.Message);
                throw ex;
            }

        }

        public bool CreateBlend(BlendInfo obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();

            var lastIncerementId = 0;
            try
            {
                var blendNo = new DocumentNo();
                if (obj.IsAutoDocNo)
                    obj.BlendNo = _documentService.GetDocumentNo(2, obj.CompanyId).FormattedDocNo;

                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendInsert";
                cmd.Parameters.AddWithValue("BlendNo", obj.BlendNo);
                cmd.Parameters.AddWithValue("ContractId", obj.ContractId);
                cmd.Parameters.AddWithValue("StandardId", obj.StandardId);
                cmd.Parameters.AddWithValue("completeDate", obj.CompleteDate);
                cmd.Parameters.AddWithValue("ExpQty", obj.ExportQty);
                cmd.Parameters.AddWithValue("User", obj.User);
                cmd.Parameters.AddWithValue("lastId", lastIncerementId);
                cmd.Parameters["lastId"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var lastIncrementedId = (Int32)cmd.Parameters["lastId"].Value;

                //Increase DocNo
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_DocumentNoGenerate";
                cmd.Parameters.AddWithValue("DocTypes", blend);
                cmd.ExecuteNonQuery();

                //Initial entry
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeInsert";
                cmd.Parameters.AddWithValue("BlendId", lastIncrementedId);
                cmd.Parameters.AddWithValue("LineQty", 0);
                cmd.Parameters.AddWithValue("Rate", 0);
                cmd.Parameters.AddWithValue("User", obj.User);
                cmd.ExecuteNonQuery();


                //Stock In
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockInsert";
                cmd.Parameters.AddWithValue("stItemId", lastIncrementedId);
                cmd.Parameters.AddWithValue("stkItemName", obj.BlendNo + " Blend Balance");
                cmd.Parameters.AddWithValue("stkName", obj.BlendNo);
                cmd.Parameters.AddWithValue("stkItemType", "BB");
                cmd.Parameters.AddWithValue("stkPack", 0);
                cmd.Parameters.AddWithValue("stkQty", 0);
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

        public bool UpdateBlend(BlendInfo obj)
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
                cmd.CommandText = "trn_BlendUpdate";
                cmd.Parameters.AddWithValue("BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("ContractId", obj.ContractId);
                cmd.Parameters.AddWithValue("StandardId", obj.StandardId);
                cmd.Parameters.AddWithValue("completeDate", obj.CompleteDate);
                cmd.Parameters.AddWithValue("ExpQty", obj.ExportQty);
                cmd.Parameters.AddWithValue("User", obj.User);
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

        public bool DeleteBlend(long blendId)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                trans = connection.BeginTransaction();
                //Delete initial entry
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeDelete";
                cmd.Parameters.AddWithValue("BlendId", blendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendDelete";
                cmd.Parameters.AddWithValue("BlendId", blendId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockDelete";
                cmd.Parameters.AddWithValue("@StktemId", blendId);
                cmd.Parameters.AddWithValue("@StockType", "BB");
                cmd.ExecuteNonQuery();

                trans.Commit();
                cmd.Dispose();
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

        public bool AddItem(BlendItem obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                ValidateBlendAddRemove(obj);
                connection.Open();
                trans = connection.BeginTransaction();

                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendItemAdd";
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@StockItemId", obj.StockId);
                cmd.Parameters.AddWithValue("@ItemName", obj.ItemName);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@StockQty", obj.BlendQty);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockRemove";
                cmd.Parameters.AddWithValue("@stkId", obj.StockId);
                cmd.Parameters.AddWithValue("@stkPacks", obj.Packs);
                cmd.Parameters.AddWithValue("@stkQty", obj.BlendQty);
                cmd.ExecuteNonQuery();

                //Update categorize line qty
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeUpdateLineQty";
                cmd.Parameters.AddWithValue("@BBlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@BLineQty", obj.BlendQty);
                cmd.ExecuteNonQuery();

                //Update Stock BB qty
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", obj.BlendQty);
                cmd.ExecuteNonQuery();

                //logs
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendLogInsert";
                cmd.Parameters.AddWithValue("@StockId", obj.StockId);
                cmd.Parameters.AddWithValue("@ItemName", obj.ItemName);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@Qty", obj.BlendQty);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@TransType", 1);
                cmd.Parameters.AddWithValue("@AllocatedBlend", obj.BlendId);
                cmd.ExecuteNonQuery();

                //Addition Deletion
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendAdditionDeletion";
                cmd.Parameters.AddWithValue("@StockId", obj.StockId);
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@Qty", obj.BlendQty);
                cmd.Parameters.AddWithValue("@TransType", 1);
                cmd.Parameters.AddWithValue("@CreateUser", obj.User);
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
            finally
            {
                connection.Close();
                connection.Dispose();
                cmd.Dispose();
            }
            return true;
        }

        public bool RemoveItem(BlendItem obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                ValidateBlendAddRemove(obj);
                connection.Open();
                trans = connection.BeginTransaction();
                //concurrent operation handled in here.Check for availability from db side
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendItemRemove";
                cmd.Parameters.AddWithValue("@BlendDetId", obj.BlendDetailId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdate";
                cmd.Parameters.AddWithValue("@stkId", obj.StockId);
                cmd.Parameters.AddWithValue("@stkPacks", obj.Packs);
                cmd.Parameters.AddWithValue("@stkQty", obj.BlendQty);
                cmd.ExecuteNonQuery();

                //Update categorize line qty
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendCategorizeUpdateLineQty";
                cmd.Parameters.AddWithValue("@BBlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@BLineQty", -obj.BlendQty);
                cmd.ExecuteNonQuery();

                //Update Stock BB qty
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "BB");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", -obj.BlendQty);
                cmd.ExecuteNonQuery();

                //logs
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendLogInsert";
                cmd.Parameters.AddWithValue("@StockId", obj.StockId);
                cmd.Parameters.AddWithValue("@ItemName", obj.ItemName);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@Qty", obj.BlendQty);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@TransType", -1);
                cmd.Parameters.AddWithValue("@AllocatedBlend", obj.BlendId);
                cmd.ExecuteNonQuery();

                //Addition Deletion
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_BlendAdditionDeletion";
                cmd.Parameters.AddWithValue("@StockId", obj.StockId);
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@Qty", obj.BlendQty);
                cmd.Parameters.AddWithValue("@TransType", -1);
                cmd.Parameters.AddWithValue("@CreateUser", obj.User);
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
            finally
            {
                connection.Close();
                connection.Dispose();
                cmd.Dispose();
            }
            return true;
        }

        public IList<StraightLineStockItem> GetStraightLIneStock(string serchString)
        {
            try
            {
                var searchBy = serchString == "all" ? "" : serchString.Replace('~', '/');
                var mapper = new DataMapper<StraightLineStockItem>("GetStraightLineStock", new Hashtable() { { "BoxNo", searchBy }, { "SearchType", 1 } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public StraighLineStockDto GetStraightLIneStockByBoxNo(string boxNo)
        {
            var stockItem = new StraighLineStockDto();
            var mapper = new DataMapper<StraightLineStockItem>("GetStraightLineStockByBoxNo", new Hashtable() { { "BoxNo", boxNo } });

            var results = mapper.ObjectList();// Mapper.QueryForList<StraightLineStockItem>("GetStraightLineStockByBoxNo", new Hashtable() { { "BoxNo", boxNo } });
            stockItem.Net = 0;
            stockItem.Pack = 0;
            stockItem.Qty = 0;
            stockItem.ReducedQty = 0;
            stockItem.NetR = 0;
            stockItem.PackR = 0;
            stockItem.QtyR = 0;
            stockItem.ReducedQtyR = 0;
            foreach (var result in results)
            {
                if (!result.IsRepeated)
                {
                    stockItem.BoxNo = result.BoxNo;
                    stockItem.Id = result.Id;
                    stockItem.StockItemId = result.StockItemId;
                    stockItem.Net = result.NetWeight;
                    stockItem.Pack = result.Packs;
                    stockItem.Qty = result.Qty;
                    stockItem.ReducedQty = result.ReducedQty;
                    stockItem.RowVersion = result.RowVersion;
                }
                else
                {
                    stockItem.BoxNo = result.BoxNo;
                    stockItem.IdR = result.Id;
                    stockItem.StockItemIdR = result.StockItemId;
                    stockItem.NetR = result.NetWeight;
                    stockItem.PackR = result.Packs;
                    stockItem.QtyR = result.Qty;
                    stockItem.ReducedQtyR = result.ReducedQty;
                    stockItem.RowVersionR = result.RowVersion;

                }
            }
            return stockItem;



        }


        public IList<BlendStockItem> GetBlendStock(string serchString)
        {
            try
            {
                var searchBy = serchString == "all" ? "" : serchString;
                var mapper = new DataMapper<BlendStockItem>("GetBlendStock", new Hashtable() { { "StockName", searchBy } });

                var result = mapper.ObjectList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public BlendStockItem GetBlendStockItemByStockId(long id)
        {
            throw new NotImplementedException();
        }

        private void ValidateBlendAddRemove(BlendItem blendItem)
        {
            if (blendItem == null)
                throw new ArgumentNullException("Empty stock item received to remove");
            if (blendItem.StockId == 0 || blendItem.StockItemId == 0 || blendItem.BlendQty == 0)
                throw new ArgumentNullException("Invalid stock item values received to remove");

        }


        public BlendHeader GetBlendInfoById(long id)
        {
            try
            {
                var mapper = new DataMapper<BlendHeader>("GetBlendInfoById", new Hashtable() { { "BlendId", id } });

                var result = mapper.Object();// Mapper.QueryForObject<BlendHeader>("GetBlendInfoById", new Hashtable() { { "BlendId", id } });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendInfoById fails. Error " + ex.Message);
                throw ex;
            }

        }


        public IList<BlendItemAllocation> GetBlendAllocationsById(long id, string itemType)
        {
            try
            {
                var mapper = new DataMapper<BlendItemAllocation>("GetBlendAllocationsById", new Hashtable() { { "BlendId", id }, { "ItemType", itemType } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendAllocationsById fails. Error " + ex.Message);
                throw ex;
            }
        }


        public IList<BlendItemAllocation> GetBlendAllocationsByBoxNo(string boxNo)
        {
            try
            {
                var mapper = new DataMapper<BlendItemAllocation>("GetBlendAllocationsByBoxNo", new Hashtable() { { "BoxNo", boxNo } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBlendAllocationsByBoxNo fails. Error " + ex.Message);
                throw ex;
            }
        }


        public bool EnableTeaBoard(BlendInfo obj)
        {
            var connection = MSSqlDataService.MsSQLConn;
            connection.Open();
            var cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "trn_BlendEnableTeaBoard";
            cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
            cmd.Parameters.AddWithValue("@User", obj.User);
            cmd.ExecuteNonQuery();
            return true;
        }


        public IList<BlendQuickSearchItem> GetBlendQuickSearchItem(string boxno)
        {
            var mapper = new DataMapper<BlendQuickSearchItem>("GetBlendQuickSearchItems", new Hashtable() { });
            var result = mapper.ObjectList();
            return result;

        }
    }
}
