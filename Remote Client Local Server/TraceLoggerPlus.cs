using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ASCOM.Utilities;

namespace ASCOM.Remote
{
    public class TraceLoggerPlus : TraceLogger
    {
        private const string ID_FORMAT_STRING = "  {0,-6} {1,-6} {2,-6}";
        private const string MESSAGE_FORMAT_STRING = "{0,-22} {1}";

        public TraceLoggerPlus(string fileName, string logName) : base(fileName, logName)
        {
        }

        public void LogMessage(int clientID, string prefix, string message)
        {
            //base.LogMessage(prefix + " " + instance.ToString(), message);
            base.LogMessage(prefix, string.Format("{0,-6} {1}", clientID, message));
        }

        public void LogMessage(int clientID, int clientTransactionID, int serverTransactionID, string prefix, string message)
        {
            //if (prefix != SharedConstants.REQUEST_RECEIVED_STRING) prefix = "  " + prefix;
            base.LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING, prefix, message));
        }

        public void LogMessageCrLf(int instance, string prefix, string message)
        {
            base.LogMessageCrLf(prefix + " " + instance.ToString(), message);
        }

        public void LogMessageCrLf(int clientID, int clientTransactionID, int serverTransactionID, string prefix, string message)
        {
            base.LogMessageCrLf(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING, prefix, message));
        }

        public bool DebugTraceState { get; set; }

    }


}
