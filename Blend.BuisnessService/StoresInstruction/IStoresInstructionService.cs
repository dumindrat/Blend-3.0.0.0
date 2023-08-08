using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.StoresInstruction;

namespace Blend.BuisnessService.StoresInstruction
{
    public interface IStoresInstructionService
    {
        IEnumerable<StoresInstructions> GetStoresInstructionsByBlend(int blendId);
        bool AddInstruction(StoresInstructions instruction);
        bool RemoveInstruction(int InstructionId);
    }
}
