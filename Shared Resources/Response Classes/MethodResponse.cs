using System;

namespace ASCOM.Remote
{
    public enum MethodTypes
    {
        PropertyGet,
        PropertySet,
        Method,
        Function
    }

    public class MethodResponse : RestResponseBase
    {
        public MethodResponse() { }
        public MethodResponse(int clientTransactionID, int transactionID)
        {
            base.ServerTransactionID = transactionID;
            base.ClientTransactionID = clientTransactionID; 
        }
        // No additional fields for this class
    }
}
