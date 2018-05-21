using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public class ActiveObject
    {
        object deviceObject;
        bool allowConnectedSetFalse;
        bool allowConnectedSetTrue;

        public ActiveObject(object deviceObjectParm,bool allowConnectedSetFalseParm,bool allowSetConnectedTrueParm)
        {
            deviceObject = deviceObjectParm;
            allowConnectedSetFalse = allowConnectedSetFalseParm;
            allowConnectedSetTrue = allowSetConnectedTrueParm;
        }

        public object DeviceObject
        {
            get => deviceObject;
            set => deviceObject = value;
        }

        public bool AllowConnectedSetFalse
        {
            get => allowConnectedSetFalse;
            set => allowConnectedSetFalse = value;
        }

        public bool AllowConnectedSetTrue
        {
            get => allowConnectedSetTrue;
            set => allowConnectedSetTrue = value;
        }

    }
}
