using System;

namespace ASCOM.Remote
{
    public class DateTimeResponse : RestResponseBase
    {
        public DateTimeResponse() { }

        public DateTimeResponse(int clientTransactionID, int transactionID, string method, DateTime value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public DateTime Value { get; set; }
    }
}
