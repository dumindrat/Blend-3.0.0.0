using Blend.BuisnessEntities.Email;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TransactionOwners
{
    public class TransactionOwnersService : BaseManagement<Transactions>, ITransactionOwnersService
    {
        public IEnumerable<Transactions> GetEmailTransactions(string search)
        {
            try
            {
                var searchBy = search == "all" ? "" : search;
                var result = Mapper.QueryForList<Transactions>("GetEmailTransactions", new Hashtable() { { "Name", searchBy } });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetEmailTransactions fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool AddOwnerToTransaction(TransactionOwner obj)
        {
            dataOperation.CommandText("sys_TransactionOwnersInsert");
            dataOperation.AddParam("@TransId", obj.TransId);
            dataOperation.AddParam("@UserId", obj.UserId);
            dataOperation.AddParam("@CreateUser", obj.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool RemoveOwnerFromTransaction(TransactionOwner obj)
        {
            dataOperation.CommandText("sys_TransactionOwnersDelete");
            dataOperation.AddParam("@TransId", obj.TransId);
            dataOperation.AddParam("@UserId", obj.UserId);
            dataOperation.DoExecute();
            return true;
        }


        public IEnumerable<TransactionOwner> GetAddedOwners(int id)
        {
            try
            {
                var result = Mapper.QueryForList<TransactionOwner>("GetAddedOwners", new Hashtable() { { "Id", id } });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetAddedOwners fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }


        public IEnumerable<TransactionOwner> GetOwners(int id)
        {
            try
            {
                var result = Mapper.QueryForList<TransactionOwner>("GetOwners", new Hashtable() { { "Id", id } });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetOwners fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }
    }
}
