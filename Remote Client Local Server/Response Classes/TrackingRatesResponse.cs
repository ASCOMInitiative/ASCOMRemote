using System;
using System.Collections.Generic;
using ASCOM.DeviceInterface;

namespace ASCOM.Remote
{
    public class TrackingRatesResponse : RestResponseBase
    {
        private List<DriveRates> rates;

        public TrackingRatesResponse() { }

        public TrackingRatesResponse(int clientTransactionID, int transactionID, string method)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            base.ClientTransactionID = clientTransactionID;
        }

        public List<DriveRates> Rates
        {
            get { return rates; }
            set { rates = value; }
        }
    }
}
