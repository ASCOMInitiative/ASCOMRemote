using System;

namespace ASCOM.Remote
{
    public class ShortResponse : RestResponseBase
    {
        public ShortResponse() { }

        public ShortResponse(int clientTransactionID, int transactionID, string method, short value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public short Value { get; set; }
    }
}
