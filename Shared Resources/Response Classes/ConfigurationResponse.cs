using System.Collections.Generic;

namespace ASCOM.Remote
{
    public class ConfigurationResponse : RestResponseBase
    {
        private Dictionary<string, ConfiguredDevice> list;

        public ConfigurationResponse() { }

        public ConfigurationResponse(int clientTransactionID, int transactionID, string method, Dictionary<string, ConfiguredDevice> value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            list = value;
            base.ClientTransactionID = clientTransactionID;
        }

        public Dictionary<string, ConfiguredDevice> Value
        {
            get { return list; }
            set { list = value; }
        }
    }
}