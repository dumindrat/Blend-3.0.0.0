using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.StoresInstruction;

namespace Blend.BuisnessService.StoresInstruction
{
    public class StoresInstructionService : BaseManagement<StoresInstructions>, IStoresInstructionService
    {
        public bool AddInstruction(BuisnessEntities.StoresInstruction.StoresInstructions instruction)
        {
            dataOperation.CommandText("trn_BlendStoresInsructionInsert");
            dataOperation.AddParam("@BlendId", instruction.BlendId);
            dataOperation.AddParam("@Instruction", instruction.StoresInstruction);
            dataOperation.DoExecute();
            return true;
        }

        public bool RemoveInstruction(int InstructionId)
        {
            dataOperation.CommandText("trn_BlendStoresInsructionDelete");
            dataOperation.AddParam("@Id", InstructionId);
            dataOperation.DoExecute();
            return true;
        }

        public IEnumerable<BuisnessEntities.StoresInstruction.StoresInstructions> GetStoresInstructionsByBlend(int blendId)
        {
            var mapper = new DataMapper<StoresInstructions>("GetStoresInstructionByBlend", new Hashtable() { { "BlendId", blendId } });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
