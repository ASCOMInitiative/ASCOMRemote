using System;

namespace ASCOM.Remote
{
    public class IntResponse : RestResponseBase
    {
        public IntResponse() { }

        public IntResponse(int clientTransactionID, int transactionID, int value)
        {
            base.ServerTransactionID = transactionID;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public int Value { get; set; }
    }
}
