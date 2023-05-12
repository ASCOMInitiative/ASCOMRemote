using System.Collections.Generic;
using ASCOM.Common.DeviceInterfaces;

namespace ASCOM.Remote
{
    public class TrackingRatesResponse : RestResponseBase
    {
        private List<DriveRate> rates;

        public TrackingRatesResponse() { }

        public TrackingRatesResponse(uint clientTransactionID, uint transactionID)
        {
            base.ServerTransactionID = transactionID;
            base.ClientTransactionID = clientTransactionID;
        }

        public List<DriveRate> Value
        {
            get { return rates; }
            set { rates = value; }
        }
    }
}
