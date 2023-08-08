using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.StoresInstruction
{
    public class StoresInstructions : BaseEntity
    {
        public int Id { get; set; }
        public int BlendId { get; set; }
        public string StoresInstruction { get; set; }
    }
}
