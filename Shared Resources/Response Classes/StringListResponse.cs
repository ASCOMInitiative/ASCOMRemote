using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public class StringListResponse:RestResponseBase
    {
        private List<string> list;

        public StringListResponse() { }

        public StringListResponse(int clientTransactionID, int transactionID, string method, List<string> value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            list = value;
            base.ClientTransactionID = clientTransactionID;
        }

        public List<string> Value
        {
            get { return list; }
            set { list = value; }
        }
    }
}
