using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
namespace ASCOM.Remote
{
    public class ActiveObject
    {
        dynamic deviceObject;
        bool allowConnectedSetFalse;
        bool allowConnectedSetTrue;
        DriverHostForm driverHostForm;

        //static readonly object commandLock;
        readonly object commandLock;

        /*        static ActiveObject()
                {
                    commandLock = new object();
                }
        */

        public ActiveObject()
        {
            commandLock = new object();
        }


        public ActiveObject(dynamic deviceObjectParm, bool allowConnectedSetFalseParm, bool allowSetConnectedTrueParm) : this()
        {
            deviceObject = deviceObjectParm;
            allowConnectedSetFalse = allowConnectedSetFalseParm;
            allowConnectedSetTrue = allowSetConnectedTrueParm;
        }

        public dynamic DeviceObject
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
        public object CommandLock
        {
            get => commandLock;
        }
        public DriverHostForm DriverHostForm
        {
            get => driverHostForm;
            set => driverHostForm = value;
        }
    }
}
