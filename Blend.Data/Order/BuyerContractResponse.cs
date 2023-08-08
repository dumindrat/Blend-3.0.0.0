using System.Collections.Generic;
using Blend.Data.Response;

namespace Blend.Data.Order
{
    public class BuyerContractResponse
    {
        public BuyerContract BuyerContractData { get; set; }
        public ResponseData Message { get; set; }
    }

    public class BuyerContractDetailResponse
    {
        public IList<BuyerContractDetail> BuyerContractDetailData { get; set; }
        public ResponseData Message { get; set; }
    }
}
