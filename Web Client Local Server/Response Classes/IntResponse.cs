using System;

namespace ASCOM.Web
{
    public class IntResponse : RestResponseBase
    {
        public IntResponse() { }

        public IntResponse(int clientTransactionID, int transactionID, string method, int value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public int Value { get; set; }
    }
}
