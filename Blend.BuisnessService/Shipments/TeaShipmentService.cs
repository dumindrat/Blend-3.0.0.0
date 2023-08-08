using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Shipments;
using Blend.DataModel.DataOperation;

namespace Blend.BuisnessService.Shipments
{
    public class TeaShipmentService : BaseManagement<TeaShipmentDto>, ITeaShipmentService
    {
        public IEnumerable<TeaShipmentDto> GetShipmentsByBlendId(int id)
        {
            throw new NotImplementedException();
        }

        public bool ShipTea(TeaShipmentDto obj)
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
                cmd.CommandText = "trn_TeaShipmentInsert";
                cmd.Parameters.AddWithValue("@BlendId", obj.BlendId);
                cmd.Parameters.AddWithValue("@Qty", obj.Qty);
                cmd.Parameters.AddWithValue("@Packs", obj.Packs);
                cmd.Parameters.AddWithValue("@ExportType", obj.ExportType);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.Parameters.AddWithValue("@RowVersion", obj.RowVersion);

                cmd.ExecuteNonQuery();

                //Update Stock BB qty
                cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "trn_TeaStockUpdateByStockIdAndType";
                cmd.Parameters.AddWithValue("@stkItemId", obj.BlendId);
                cmd.Parameters.AddWithValue("@stkType", "STOCK");
                cmd.Parameters.AddWithValue("@stkPacks", 0);
                cmd.Parameters.AddWithValue("@stkQty", -obj.Qty);
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

        public IEnumerable<ClosedBlend> GetClosedBlends(string blendNo)
        {
            var searchBy = blendNo == "all" ? "" : blendNo;
            var mapper = new DataMapper<ClosedBlend>("GetClosedBlends", new Hashtable() { { "BlendNo", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
