using System;

namespace ASCOM.Remote
{
    public class ShortResponse : RestResponseBase
    {
        public ShortResponse() { }

        public ShortResponse(int clientTransactionID, int transactionID, short value)
        {
            base.ServerTransactionID = transactionID;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public short Value { get; set; }
    }
}
