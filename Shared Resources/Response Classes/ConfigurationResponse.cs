using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ASCOM.Remote
{
    public class ConfigurationResponse : RestResponseBase
    {
        private ConcurrentDictionary<string, ConfiguredDevice> list;

        public ConfigurationResponse() { }

        public ConfigurationResponse(int clientTransactionID, int transactionID, string method, ConcurrentDictionary<string, ConfiguredDevice> value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            list = value;
            base.ClientTransactionID = clientTransactionID;
        }

        public ConcurrentDictionary<string, ConfiguredDevice> Value
        {
            get { return list; }
            set { list = value; }
        }
    }
}