using System;
using System.Collections;
using Blend.Data.Stock;

namespace Blend.Data.Purchase
{
    public class StraightLineManagement : BaseManagement<StraightLineDataModel>
    {
        public void Create(StraightLineDataModel obj)
        {
            Mapper.BeginTransaction();
            foreach (var straightLine in obj.StraightLines)
            {
                Mapper.Insert("InsertStraightLine", straightLine);
            }
            const int docType = 1;
            Mapper.Update("UpdatetraightLineDocNo", docType);
            Mapper.CommitTransaction();

        }

        public void Update(StraightLineDataModel obj)
        {
            if (obj == null)
                throw new ApplicationException("Invalid reuquest object to execute");

            try
            {
                Mapper.BeginTransaction();
                ValidateStraightLine(obj.StraightLines[0]);
                Mapper.Update("UpdateStraightLine", obj.StraightLines[0]);
                Mapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                Mapper.RollBackTransaction();
                //ToDo: Log error
                throw new ApplicationException(string.Format("Error occured in Update data: {0}. Error : {1}", obj.StraightLines[0].ReservedNo, ex.Message));
            }
        }

        public  StraightLineDataModel GetDataById(int key)
        {

            throw new NotImplementedException();
        }

        public void Delete(StraightLineDataModel obj)
        {
            if (obj == null)
                throw new ApplicationException("Invalid reuquest object to execute");

            try
            {
                Mapper.BeginTransaction();
                ValidateStraightLine(obj.StraightLines[0]);
                Mapper.Delete("DeleteStraightLine", obj.StraightLines[0]);
                Mapper.CommitTransaction();
            }
            catch (Exception ex)
            {
                Mapper.RollBackTransaction();
                //ToDo: Log error
                throw new ApplicationException(string.Format("Error occured in Delete data: {0}. Error : {1}", obj.StraightLines[0].ReservedNo, ex.Message));
            }
        }

        public StraightLine GetStraightLineById(string id)
        {
            var data = Mapper.QueryForObject<StraightLine>("GetStraightLineById", id);
            return data;
        }

        private static void ValidateStraightLine(StraightLine obj)
        {
            var stock = new StockDataManagement().GetStockDataByReservedNoAndCategory(obj.ReservedNo, "L", obj.Id);
            if (stock == null)
                throw new ApplicationException(string.Format("Stock Qty not found for the Line :{0}", obj.ReservedNo));

            var purchase = new StraightLineManagement().GetStraightLineById(obj.Id.ToString());
            if (purchase.Weight != stock.Qty)
                throw new ApplicationException(string.Format("Stock Qty mismatches found for the Line :{0}", obj.ReservedNo));
        }

        public  System.Collections.Generic.IList<StraightLineDataModel> GetDataList(long key)
        {
            throw new NotImplementedException();
        }
    }
}
