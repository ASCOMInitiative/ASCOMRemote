using System.Collections.Generic;

namespace ASCOM.Remote
{
    public class ConfigurationResponse : RestResponseBase
    {
        private Dictionary<string, ConfiguredDevice> list;

        public ConfigurationResponse() { }

        public ConfigurationResponse(uint clientTransactionID, uint transactionID, Dictionary<string, ConfiguredDevice> value)
        {
            base.ClientTransactionID = clientTransactionID;
            base.ServerTransactionID = transactionID;
            list = value;
        }

        public Dictionary<string, ConfiguredDevice> Value
        {
            get { return list; }
            set { list = value; }
        }
    }
}