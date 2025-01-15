using System.Collections.Generic;

namespace ASCOM.Remote
{
    public class AlpacaConfiguredDevicesResponse : RestResponseBase
    {
        public AlpacaConfiguredDevicesResponse()
        {
            Value = [];
        }

        public AlpacaConfiguredDevicesResponse(uint clientTransactionID, uint transactionID, List<AlpacaConfiguredDevice> value)
        {
            base.ServerTransactionID = transactionID;
            base.ClientTransactionID = clientTransactionID;
            Value = value;
        }

        public List<AlpacaConfiguredDevice> Value { get; set; }
    }
}
