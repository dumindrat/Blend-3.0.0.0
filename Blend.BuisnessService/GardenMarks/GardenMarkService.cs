using Blend.BuisnessEntities.Purchase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.GardenMarks
{
    public class GardenMarkService : BaseManagement<GardenMark>, IGardenMarkService
    {
        public IEnumerable<GardenMark> GetGardenMarksList()
        {
            try
            {
                var mapper = new DataMapper<GardenMark>("GetGardenMarks", new Hashtable() { });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetGardenMarksList fails. Error " + ex.Message);
                throw ex;
            }
        }

        public bool InsertGardenMark(string gardenMark)
        {
            dataOperation.CommandText("trn_GardenMarkInsert");
            dataOperation.AddParam("@GardenMark", gardenMark);
            dataOperation.DoExecute();
            return true;
        }
    }
}
