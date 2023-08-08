using Blend.BuisnessEntities.Contract;
using Blend.BuisnessEntities.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Contract
{
    public interface IContractService
    {
        IEnumerable<BuyerContract> GetContractList(string serchStr);
        IEnumerable<BuyerContract> GetOpenContractList();
        bool CreateContract(BuyerContract obj);
        bool UpdateContract(BuyerContract obj);
        bool DeleteContract(BuyerContract obj);
        BuyerContract GetContractById(int id);
        IEnumerable<BuyerContractDetail> GetBuyerContractDetailById(long key);
        IList<TeaStandardForContract> GetAvailableStandardsById(long key);
        IList<TeaStandardForContract> GetAvailableStandardsByIdAndSearchStr(long key,string searchstr);
        bool CreateBuyerContractDetail(BuyerContractDetail obj);
        bool UpdateBuyerContractDetail(BuyerContractDetail obj);
        bool DeleteBuyerContractDetail(BuyerContractDetail obj);
        IEnumerable<BlendNoByCon> GetBlendListByConAndStanList(int contractId,int standardId);

    }
}
