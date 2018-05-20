using System;

namespace ASCOM.Remote
{
    public class StringResponse : RestResponseBase
    {
        public StringResponse() { }

        public StringResponse(int clientTransactionID, int transactionID, string method, string value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public string Value { get; set; }
    }
}
