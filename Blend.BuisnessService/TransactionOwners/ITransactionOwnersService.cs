using Blend.BuisnessEntities.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TransactionOwners
{
    public interface ITransactionOwnersService
    {
        IEnumerable<Transactions> GetEmailTransactions(string search);
        IEnumerable<TransactionOwner> GetAddedOwners(int id);
        IEnumerable<TransactionOwner> GetOwners(int id);
        bool AddOwnerToTransaction(TransactionOwner obj);
        bool RemoveOwnerFromTransaction(TransactionOwner obj);

    }
}
