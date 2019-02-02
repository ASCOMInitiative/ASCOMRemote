using ASCOM.Utilities;

namespace ASCOM.Remote
{
    public class TraceLoggerPlus : TraceLogger
    {
        private const string ID_FORMAT_STRING = "  {0,6} {1,6} {2,6}";
        private const string MESSAGE_FORMAT_STRING_NO_IP_ADDRESS = "{0,-22} {1}";
        private const string MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS = "{0,-22} {1,-22} {2}";

        public TraceLoggerPlus(string fileName, string logName) : base(fileName, logName)
        {
            IpAddressTraceState = true;
        }

        public bool DebugTraceState { get; set; }

        public bool IpAddressTraceState { get; set; }

        public void LogMessage(int instance, string prefix, string message)
        {
            base.LogMessage(prefix + " " + instance.ToString(), message);
        }

        public void LogMessage(int clientID, int clientTransactionID, int serverTransactionID, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                base.LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
            else
            {
                base.LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessage(RequestData requestData, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                base.LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, requestData.ClientIpAddress, prefix, message));
            }
            else
            {
                base.LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessageCrLf(int instance, string prefix, string message)
        {
            base.LogMessageCrLf(prefix + " " + instance.ToString(), message);
        }

        public void LogMessageCrLf(int clientID, int clientTransactionID, int serverTransactionID, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                base.LogMessageCrLf(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, "", prefix, message));
            }
            else
            {
                base.LogMessageCrLf(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessageCrLf(RequestData requestData, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                base.LogMessageCrLf(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, requestData.ClientIpAddress, prefix, message));
            }
            else
            {
                base.LogMessageCrLf(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }
    }
}