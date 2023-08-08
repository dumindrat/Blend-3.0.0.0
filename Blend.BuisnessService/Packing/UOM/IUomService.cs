using Blend.BuisnessEntities.Packing.UnitOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.UOM
{
    public interface IUomService
    {
        IList<Uom> GetUomList();
    }
}
