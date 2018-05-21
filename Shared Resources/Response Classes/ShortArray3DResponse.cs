using System;

namespace ASCOM.Remote
{
    public class ShortArray3DResponse : ImageArrayResponseBase
    {
        private short[,,] shortArray3D;

        private const int RANK = 3;
        private const SharedConstants.ImageArrayElementTypes TYPE = SharedConstants.ImageArrayElementTypes.Short;

        public ShortArray3DResponse(int clientTransactionID, int transactionID, string method, short[,,] value)
        {
            base.ServerTransactionID = transactionID;
            base.Method = method;
            shortArray3D = value;
            base.Type = (int)TYPE;
            base.Rank = RANK;
            base.ClientTransactionID = clientTransactionID;
        }

        public short[,,] Value
        {
            get { return shortArray3D; }
            set
            {
                shortArray3D = value;
                base.Type = (int)TYPE;
                base.Rank = RANK;
            }
        }
    }
}
