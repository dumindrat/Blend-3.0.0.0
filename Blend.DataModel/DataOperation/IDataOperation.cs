using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.DataModel.DataOperation
{
    public interface IDataOperation
    {
        void CommandText(string commandText);
        void DoExecute();
        void AddParam(string paramName, object value);
        void BeginTrans();
        void CommitTrans();
        void RollBackTrans();
        void ConnectionClose();

    }
}
