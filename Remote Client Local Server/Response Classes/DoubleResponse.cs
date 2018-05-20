using System;

namespace ASCOM.Web
{
    public class DoubleResponse : RestResponseBase
    {
        public DoubleResponse() { }

        public DoubleResponse(int clientTransactionID, int transactionID, string method, double value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public double Value { get; set; }
    }
}
