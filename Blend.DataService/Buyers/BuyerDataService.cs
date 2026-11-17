using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Buyers
{
    public sealed class BuyerDataService : IBuyerDataService
    {
        private readonly IStoredProcedureMapper _mapper;

        public BuyerDataService(IStoredProcedureMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public IReadOnlyList<BuyerRecord> GetAllBuyers(string searchText, int companyId)
        {
            var searchBy = searchText == "all" ? string.Empty : searchText;
            return _mapper.Query<BuyerRecord>("trn_BuyerGet", new { serachstring = searchBy, CompanyId = companyId });
        }

        public bool CreateBuyer(BuyerRecord buyer)
        {
            _mapper.Execute("trn_BuyerInsert", new
            {
                BuyerCode = buyer.BuyerCode,
                BuyerName = buyer.BuyerName,
                Address = buyer.Address,
                Email = buyer.Email,
                User = buyer.User,
                CompanyId = buyer.CompanyId
            });
            return true;
        }

        public bool UpdateBuyer(BuyerRecord buyer)
        {
            _mapper.Execute("trn_BuyerUpdate", new
            {
                BuyerCode = buyer.BuyerCode,
                BId = buyer.BuyerId,
                TeaBuyerName = buyer.BuyerName,
                BAddress = buyer.Address,
                BEmail = buyer.Email,
                BRowVersion = buyer.RowVersion
            });
            return true;
        }

        public bool DeleteBuyer(int buyerId)
        {
            _mapper.Execute("trn_BuyerDelete", new { Ids = buyerId });
            return true;
        }
    }
}
