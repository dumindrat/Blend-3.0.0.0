using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Shipments;

namespace Blend.BuisnessService.Shipments
{
    public interface ITeaShipmentService
    {
        IEnumerable<ClosedBlend> GetClosedBlends(string blendNo);
        IEnumerable<TeaShipmentDto> GetShipmentsByBlendId(int id);
        bool ShipTea(TeaShipmentDto obj);
    }
}
