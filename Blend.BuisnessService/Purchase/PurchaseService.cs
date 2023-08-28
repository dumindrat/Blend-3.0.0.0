using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Document;
using Blend.BuisnessService.GardenMarks;
using Blend.DataModel.DataOperation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessService.Grades;

namespace Blend.BuisnessService.Purchase
{
    public class PurchaseService : BaseManagement<Sale>, IPurchaseService
    {
        private IDocumentService _documentService;
        private IGardenMarkService _gardenMarkService;
        private IGradeService _gradeService;
        //private MSSqlDataService _dataSvc;
        public PurchaseService(IDocumentService documentService, IGardenMarkService gardenMarkService, IGradeService gradeService)
        {
            _documentService = documentService;
            _gardenMarkService = gardenMarkService;
            _gradeService = gradeService;
        }

        private void SaveGardenMark(string gardenMark)
        {
            try
            {
                _gardenMarkService.InsertGardenMark(gardenMark);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
        }

        private void SaveGrade(string grade)
        {
            try
            {
                _gradeService.InsertGrade(grade);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
        }

        public IEnumerable<Sale> GetAllSales(string searchString, int companyId)
        {
            try
            {
                var searchBy = searchString == "all" ? "" : searchString;
                var mapper = new DataMapper<Sale>("GetSales", new Hashtable() { { "SaleDate", searchBy }, { "CompanyId", companyId } });

                var results = mapper.ObjectList();
                return results;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetAllSales fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool CreateSale(Sale SaleEntity)
        {
            dataOperation.CommandText("trn_PurchaseHeaderInsert");
            dataOperation.AddParam("@SaleDates", SaleEntity.SaleDate);
            dataOperation.AddParam("@User", SaleEntity.User);
            dataOperation.AddParam("@CompanyId", SaleEntity.CompanyId);
            dataOperation.DoExecute();
            return true;
        }

        public bool AddStraightLine(StraightLine StrightLineEntity)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            var lastIncerementId = 0;
            try
            {
                var boxNo = _documentService.GetDocumentNo(1, StrightLineEntity.CompanyId);
                connection.Open();
                trans = connection.BeginTransaction();

                //cmd = new MySqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TinNoUpdate";
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_PurchaseDetailInsert";
                cmd.Parameters.AddWithValue("BoxNos", boxNo.FormattedDocNo);
                cmd.Parameters.AddWithValue("SaleIds", StrightLineEntity.SaleId);
                cmd.Parameters.AddWithValue("SaleCodes", StrightLineEntity.SaleCode);
                cmd.Parameters.AddWithValue("Invoices", StrightLineEntity.Invoice);
                cmd.Parameters.AddWithValue("LotNos", StrightLineEntity.LotNo);
                cmd.Parameters.AddWithValue("BrokerIds", StrightLineEntity.BrokerId);
                cmd.Parameters.AddWithValue("SaleTypes", StrightLineEntity.SaleType);
                cmd.Parameters.AddWithValue("Grades", StrightLineEntity.Grade);
                cmd.Parameters.AddWithValue("Standards", String.IsNullOrEmpty(StrightLineEntity.Standard) == true ? string.Empty : StrightLineEntity.Standard);
                cmd.Parameters.AddWithValue("GardenMarks", StrightLineEntity.GardenMark);
                cmd.Parameters.AddWithValue("Net", StrightLineEntity.Net1);
                cmd.Parameters.AddWithValue("Pakets", StrightLineEntity.Pack1);
                decimal? reducedqty = 0;
                reducedqty = (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0) ? 0 : StrightLineEntity.ReducedQty;
                decimal? lineOneWeight = 0;
                lineOneWeight = StrightLineEntity.Net1 * StrightLineEntity.Pack1 - reducedqty;
                cmd.Parameters.AddWithValue("Weights", lineOneWeight);
                cmd.Parameters.AddWithValue("ReducedQty", reducedqty);
                cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                cmd.Parameters.AddWithValue("Price", lineOneWeight * StrightLineEntity.Rate);
                //cmd.Parameters.AddWithValue("TinNos", StrightLineEntity.TinNo);
                cmd.Parameters.AddWithValue("User", StrightLineEntity.User);
                cmd.Parameters.AddWithValue("Repeated", false);
                decimal? totalWeight = 0;
                //This is for reporting purposes
                if (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0)
                {
                    totalWeight = (StrightLineEntity.Net1 * StrightLineEntity.Pack1) + (StrightLineEntity.Net2 * StrightLineEntity.Pack2 - StrightLineEntity.ReducedQty);
                    cmd.Parameters.AddWithValue("VirtualWeights", totalWeight);
                    cmd.Parameters.AddWithValue("VirtualValues", totalWeight * StrightLineEntity.Rate);
                }
                else
                {
                    //totalWeight = (StrightLineEntity.Net1 * StrightLineEntity.Pack1);
                    cmd.Parameters.AddWithValue("VirtualWeights", lineOneWeight);
                    cmd.Parameters.AddWithValue("VirtualValues", lineOneWeight * StrightLineEntity.Rate);
                }
                cmd.Parameters.AddWithValue("Certificates", StrightLineEntity.TeaCertificates);
                cmd.Parameters.AddWithValue("lastId", lastIncerementId);
                cmd.Parameters["lastId"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var lastIncrementedId = (Int32)cmd.Parameters["lastId"].Value;

                //Stock In
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockInsert";
                cmd.Parameters.AddWithValue("stItemId", lastIncrementedId);
                cmd.Parameters.AddWithValue("stkItemName", boxNo.FormattedDocNo);
                cmd.Parameters.AddWithValue("stkName", boxNo.FormattedDocNo);
                cmd.Parameters.AddWithValue("stkItemType", "L");
                cmd.Parameters.AddWithValue("stkPack", StrightLineEntity.Pack1);
                cmd.Parameters.AddWithValue("stkQty", lineOneWeight);
                cmd.ExecuteNonQuery();


                //Repeating lines
                if (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_PurchaseDetailInsert";
                    cmd.Parameters.AddWithValue("BoxNos", boxNo.FormattedDocNo);
                    cmd.Parameters.AddWithValue("SaleIds", StrightLineEntity.SaleId);
                    cmd.Parameters.AddWithValue("SaleCodes", StrightLineEntity.SaleCode);
                    cmd.Parameters.AddWithValue("Invoices", StrightLineEntity.Invoice);
                    cmd.Parameters.AddWithValue("LotNos", StrightLineEntity.LotNo);
                    cmd.Parameters.AddWithValue("BrokerIds", StrightLineEntity.BrokerId);
                    cmd.Parameters.AddWithValue("SaleTypes", StrightLineEntity.SaleType);
                    cmd.Parameters.AddWithValue("Grades", StrightLineEntity.Grade);
                    cmd.Parameters.AddWithValue("Standards", String.IsNullOrEmpty(StrightLineEntity.Standard) == true ? string.Empty : StrightLineEntity.Standard);
                    cmd.Parameters.AddWithValue("GardenMarks", StrightLineEntity.GardenMark);
                    cmd.Parameters.AddWithValue("Net", StrightLineEntity.Net2);
                    cmd.Parameters.AddWithValue("Pakets", StrightLineEntity.Pack2);
                    decimal? repeatingLineWeight = 0;
                    repeatingLineWeight = StrightLineEntity.Net2 * StrightLineEntity.Pack2 - StrightLineEntity.ReducedQty;

                    cmd.Parameters.AddWithValue("Weights", repeatingLineWeight);
                    cmd.Parameters.AddWithValue("ReducedQty", StrightLineEntity.ReducedQty);
                    cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                    cmd.Parameters.AddWithValue("Price", repeatingLineWeight * StrightLineEntity.Rate);
                    //cmd.Parameters.AddWithValue("TinNos", StrightLineEntity.TinNo);
                    cmd.Parameters.AddWithValue("User", StrightLineEntity.User);
                    cmd.Parameters.AddWithValue("Repeated", true);
                    cmd.Parameters.AddWithValue("VirtualWeights", 0);
                    cmd.Parameters.AddWithValue("VirtualValues", 0);
                    cmd.Parameters.AddWithValue("Certificates", StrightLineEntity.TeaCertificates);
                    cmd.Parameters.AddWithValue("lastId", lastIncerementId);
                    cmd.Parameters["lastId"].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    var lastRepeatingLineAutoId = (Int32)cmd.Parameters["lastId"].Value;

                    //Stock In
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockInsert";
                    cmd.Parameters.AddWithValue("stItemId", lastRepeatingLineAutoId);
                    cmd.Parameters.AddWithValue("stkItemName", boxNo.FormattedDocNo);
                    cmd.Parameters.AddWithValue("stkName", boxNo.FormattedDocNo);
                    cmd.Parameters.AddWithValue("stkItemType", "L");
                    cmd.Parameters.AddWithValue("stkPack", StrightLineEntity.Pack2);
                    cmd.Parameters.AddWithValue("stkQty", repeatingLineWeight);
                    cmd.ExecuteNonQuery();
                }
                //Increase DocNo
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_DocumentNoGenerate";
                cmd.Parameters.AddWithValue("@DocTypes", 1);
                cmd.Parameters.AddWithValue("@CompanyId", StrightLineEntity.CompanyId);
                cmd.ExecuteNonQuery();

                trans.Commit();
                cmd.Dispose();

                SaveGardenMark(StrightLineEntity.GardenMark);
                SaveGrade(StrightLineEntity.Grade);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                connection.Close();
                cmd.Dispose();
            }
            return true;
        }

        public bool UpdateStraightLine(StraightLine StrightLineEntity)
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
                cmd.CommandText = "trn_PrurchaseDetailUpdate";
                cmd.Parameters.AddWithValue("pId", StrightLineEntity.Id);
                cmd.Parameters.AddWithValue("Stockid", StrightLineEntity.StockId);
                cmd.Parameters.AddWithValue("SaleIds", StrightLineEntity.SaleId);
                cmd.Parameters.AddWithValue("SaleCodes", StrightLineEntity.SaleCode);
                cmd.Parameters.AddWithValue("Invoices", StrightLineEntity.Invoice);
                cmd.Parameters.AddWithValue("LotNos", StrightLineEntity.LotNo);
                cmd.Parameters.AddWithValue("BrokerIds", StrightLineEntity.BrokerId);
                cmd.Parameters.AddWithValue("SaleTypes", StrightLineEntity.SaleType);
                cmd.Parameters.AddWithValue("Grades", StrightLineEntity.Grade);
                cmd.Parameters.AddWithValue("Standards", StrightLineEntity.Standard);
                cmd.Parameters.AddWithValue("GardenMarks", StrightLineEntity.GardenMark);
                cmd.Parameters.AddWithValue("Net", StrightLineEntity.Net1);
                cmd.Parameters.AddWithValue("Pakets", StrightLineEntity.Pack1);
                //cmd.Parameters.AddWithValue("Weights", StrightLineEntity.Net1 * StrightLineEntity.Pack1);
                //cmd.Parameters.AddWithValue("ReducedQty", StrightLineEntity.ReducedQty);
                //cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                decimal? reducedqty = 0;
                reducedqty = (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0) ? 0 : StrightLineEntity.ReducedQty;
                decimal? lineOneWeight = 0;
                lineOneWeight = StrightLineEntity.Net1 * StrightLineEntity.Pack1 - reducedqty;
                cmd.Parameters.AddWithValue("Weights", lineOneWeight);
                cmd.Parameters.AddWithValue("ReducedQty", reducedqty);
                cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                cmd.Parameters.AddWithValue("Price", lineOneWeight * StrightLineEntity.Rate);

                //cmd.Parameters.AddWithValue("TinNos", StrightLineEntity.TinNo);
                cmd.Parameters.AddWithValue("User", StrightLineEntity.User);
                cmd.Parameters.AddWithValue("RowVersion", StrightLineEntity.RowVersion1);
                cmd.Parameters.AddWithValue("Certificates", StrightLineEntity.TeaCertificates);

                decimal totalWeight = 0;
                //This is for reporting purposes
                if (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0)
                {
                    totalWeight = (StrightLineEntity.Net1 * StrightLineEntity.Pack1) + (StrightLineEntity.Net2 * StrightLineEntity.Pack2) - Convert.ToDecimal(StrightLineEntity.ReducedQty);
                    cmd.Parameters.AddWithValue("VirtualWeights", totalWeight);
                    cmd.Parameters.AddWithValue("VirtualValues", totalWeight * StrightLineEntity.Rate);
                }
                else
                {
                    //totalWeight = (StrightLineEntity.Net1 * StrightLineEntity.Pack1);
                    cmd.Parameters.AddWithValue("VirtualWeights", lineOneWeight);
                    cmd.Parameters.AddWithValue("VirtualValues", lineOneWeight * StrightLineEntity.Rate);
                }
                cmd.ExecuteNonQuery();

                //Stock In
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByEditLine";
                cmd.Parameters.AddWithValue("@stkId", StrightLineEntity.StockId);
                cmd.Parameters.AddWithValue("@stkPacks", StrightLineEntity.Pack1);
                cmd.Parameters.AddWithValue("@stkQty", lineOneWeight);
                cmd.ExecuteNonQuery();


                //Repeating lines
                if (StrightLineEntity.Net2 > 0 && StrightLineEntity.Pack2 > 0)
                {
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_PrurchaseDetailUpdate";
                    cmd.Parameters.AddWithValue("pId", StrightLineEntity.RepeatingId);
                    cmd.Parameters.AddWithValue("Stockid", StrightLineEntity.RepeatingStockId);
                    cmd.Parameters.AddWithValue("SaleIds", StrightLineEntity.SaleId);
                    cmd.Parameters.AddWithValue("SaleCodes", StrightLineEntity.SaleCode);
                    cmd.Parameters.AddWithValue("Invoices", StrightLineEntity.Invoice);
                    cmd.Parameters.AddWithValue("LotNos", StrightLineEntity.LotNo);
                    cmd.Parameters.AddWithValue("BrokerIds", StrightLineEntity.BrokerId);
                    cmd.Parameters.AddWithValue("SaleTypes", StrightLineEntity.SaleType);
                    cmd.Parameters.AddWithValue("Grades", StrightLineEntity.Grade);
                    cmd.Parameters.AddWithValue("Standards", StrightLineEntity.Standard);
                    cmd.Parameters.AddWithValue("GardenMarks", StrightLineEntity.GardenMark);
                    cmd.Parameters.AddWithValue("Net", StrightLineEntity.Net2);
                    cmd.Parameters.AddWithValue("Pakets", StrightLineEntity.Pack2);
                    //cmd.Parameters.AddWithValue("Weights", StrightLineEntity.Net2 * StrightLineEntity.Pack2);
                    //cmd.Parameters.AddWithValue("ReducedQty", StrightLineEntity.ReducedQty);
                    //cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                    decimal? repeatingLineWeight = 0;
                    repeatingLineWeight = StrightLineEntity.Net2 * StrightLineEntity.Pack2 - StrightLineEntity.ReducedQty;

                    cmd.Parameters.AddWithValue("Weights", repeatingLineWeight);
                    cmd.Parameters.AddWithValue("ReducedQty", StrightLineEntity.ReducedQty);
                    cmd.Parameters.AddWithValue("Rates", StrightLineEntity.Rate);
                    cmd.Parameters.AddWithValue("Price", repeatingLineWeight * StrightLineEntity.Rate);
                    //cmd.Parameters.AddWithValue("TinNos", StrightLineEntity.TinNo);
                    cmd.Parameters.AddWithValue("User", StrightLineEntity.User);
                    cmd.Parameters.AddWithValue("RowVersion", StrightLineEntity.RowVersion2);
                    cmd.Parameters.AddWithValue("VirtualWeights", 0);
                    cmd.Parameters.AddWithValue("VirtualValues", 0);
                    cmd.Parameters.AddWithValue("Certificates", StrightLineEntity.TeaCertificates);

                    cmd.ExecuteNonQuery();

                    //Stock In
                    cmd = new SqlCommand();
                    cmd.Transaction = trans;
                    cmd.Connection = connection;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "trn_TeaStockUpdateByEditLine";
                    cmd.Parameters.AddWithValue("@stkId", StrightLineEntity.RepeatingStockId);
                    cmd.Parameters.AddWithValue("@stkPacks", StrightLineEntity.Pack2);
                    cmd.Parameters.AddWithValue("@stkQty", repeatingLineWeight);
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
            finally
            {
                connection.Close();
                cmd.Dispose();
            }
            return true;
        }

        public IEnumerable<BuisnessEntities.Purchase.StraightLine> GetAddedStraighLIne(int saleId)
        {
            try
            {
                var mapper = new DataMapper<StraightLine>("GetStraightLineBySale", new StraightLine() { SaleId = saleId });

                var results = mapper.ObjectList();
                //if (results.Count > 1)
                //    results[0].ReducedQty = results[1].ReducedQty;

                foreach (var item in results.Where(w => w.IsRepeated == true))
                {
                    item.BoxNo = "";
                    item.Grade = "";
                    item.Standard = "";
                    item.SaleCode = "";
                    item.BrokerId = null;
                    item.BrokerCode = "";
                    item.SaleId = null;
                    item.SaleType = null;
                    item.Invoice = "";
                    item.LotNo = "";
                    item.GardenMark = "";
                    item.Rate = null;
                    item.Weight = null;
                    //item.ReducedQty = null;
                    item.Value = null;
                    item.VirtualValue = null;
                    item.VirtualWeight = null;
                }
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }

        }


        public StraightLine GetStraightLineByReservedNo(long straighlineId)
        {
            try
            {
                var mapper = new DataMapper<StraightLine>("GetStraightLineByReservedNoAndSale", new StraightLine() { Id = straighlineId });

                var result = mapper.ObjectList();
                var straighLineResult = new StraightLine();
                if (result != null)
                {
                    straighLineResult = (StraightLine)result[0].Clone();
                    straighLineResult.Net1 = result[0].NetWeight;
                    straighLineResult.Pack1 = result[0].Packs;
                    straighLineResult.Weight = result[0].VirtualWeight;
                    straighLineResult.Value = result[0].VirtualValue;
                    straighLineResult.RowVersion1 = result[0].RowVersion;
                    if (result.Count > 1)
                    {
                        straighLineResult.RepeatingId = result[1].Id;
                        straighLineResult.RepeatingStockId = result[1].StockId;
                        straighLineResult.Net2 = result[1].NetWeight;
                        straighLineResult.Pack2 = result[1].Packs;
                        straighLineResult.RowVersion2 = result[1].RowVersion;
                        straighLineResult.ReducedQty = result[1].ReducedQty;
                    }
                }
                return straighLineResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public IEnumerable<TeaCertificates> GetTeaCertificates()
        {
            try
            {
                var mapper = new DataMapper<TeaCertificates>("GetTeaCertificates", new Hashtable() { });

                var results = mapper.ObjectList();
                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public bool DeleteStraightLine(StraightLine StrightLineEntity)
        {
            dataOperation.CommandText("trn_PurchaseDetailDelete");
            dataOperation.AddParam("@Pid", StrightLineEntity.Id);
            dataOperation.AddParam("@reservedNo", StrightLineEntity.BoxNo);
            dataOperation.AddParam("@Stockid", StrightLineEntity.StockId);
            dataOperation.AddParam("@Reason", "");
            dataOperation.AddParam("@User", StrightLineEntity.User);
            dataOperation.DoExecute();
            return true;
        }


        public bool CloseSale(Sale SaleEntity)
        {
            dataOperation.CommandText("trn_PurchaseHeaderUpdateByClose");
            dataOperation.AddParam("@SaleId", SaleEntity.SaleId);
            dataOperation.AddParam("@Status", 1);
            dataOperation.AddParam("@User", SaleEntity.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool OpenSale(Sale SaleEntity)
        {
            dataOperation.CommandText("trn_PurchaseHeaderUpdateByClose");
            dataOperation.AddParam("@SaleId", SaleEntity.SaleId);
            dataOperation.AddParam("@Status", 0);
            dataOperation.AddParam("@User", SaleEntity.User);
            dataOperation.DoExecute();
            return true;
        }


        public bool UpdateSale(Sale SaleEntity)
        {
            dataOperation.CommandText("trn_PurchaseHeaderUpdate");
            dataOperation.AddParam("@SaleId", SaleEntity.SaleId);
            dataOperation.AddParam("@SaleDate", SaleEntity.SaleDate);
            dataOperation.AddParam("@User", SaleEntity.User);
            dataOperation.DoExecute();
            return true;
        }
    }
}
