using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Brokers
{
    public sealed class BrokerDataService : IBrokerDataService
    {
        private readonly IStoredProcedureMapper _mapper;

        public BrokerDataService(IStoredProcedureMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public BrokerRecord? GetBrokerById(int brokerId)
        {
            return _mapper.QuerySingleOrDefault<BrokerRecord>("trn_BrokerGetById", new { BrokerId = brokerId });
        }

        public IReadOnlyList<BrokerRecord> GetAllBrokers(string searchText, int companyId)
        {
            var searchBy = searchText == "all" ? string.Empty : searchText;
            return _mapper.Query<BrokerRecord>("trn_BrokerGet", new { BrokerName = searchBy, CompanyId = companyId });
        }

        public bool CreateBroker(BrokerRecord broker)
        {
            _mapper.Execute("trn_BrokerInsert", new
            {
                BCode = broker.BrokerCode,
                BName = broker.BrokerName,
                BAddress = broker.Address,
                BEmail = broker.Email,
                BTel = broker.Tel,
                BContactPerson = broker.ContactPerson,
                User = broker.User,
                CompanyId = broker.CompanyId
            });
            return true;
        }

        public bool UpdateBroker(BrokerRecord broker)
        {
            _mapper.Execute("trn_BrokerUpdate", new
            {
                BId = broker.BrokerId,
                BCode = broker.BrokerCode,
                BName = broker.BrokerName,
                BAddress = broker.Address,
                BEmail = broker.Email,
                BTel = broker.Tel,
                BContactPerson = broker.ContactPerson,
                RowVersion = broker.RowVersion,
                User = broker.User
            });
            return true;
        }

        public bool DeleteBroker(int brokerId)
        {
            _mapper.Execute("trn_BrokerDelete", new { BId = brokerId });
            return true;
        }
    }
}
