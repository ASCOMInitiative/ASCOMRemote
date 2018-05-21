using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public class IntArray1DResponse : RestResponseBase
    {
        private int[] intArray1D;

        public IntArray1DResponse() { }

        public IntArray1DResponse(int clientTransactionID, int transactionID, string method, int[] value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            intArray1D = value;
        }

        public int[] Value
        {
            get { return intArray1D; }
            set { intArray1D = value; }
        }
    }
}
