using Blend.BuisnessEntities.Packing.UnitOfMeasure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.UOM
{
    class UomService : IUomService
    {
        public IList<Uom> GetUomList()
        {
            var mapper = new DataMapper<Uom>("GetUom", new Hashtable() {  });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
