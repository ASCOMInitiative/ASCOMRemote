using System;
using System.Collections.Generic;

namespace ASCOM.Web
{
    public class AxisRatesResponse : RestResponseBase
    {
        public AxisRatesResponse() { }

        public AxisRatesResponse(int clientTransactionID, int transactionID, string method, List<RateResponse> value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public List<RateResponse> Value { get; set; }
    }
}