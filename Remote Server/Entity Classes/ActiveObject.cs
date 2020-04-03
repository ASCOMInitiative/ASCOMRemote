using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
namespace ASCOM.Remote
{
    /// <summary>
    /// Class to hold information about an ASCOM Remote served device
    /// </summary>
    public class ActiveObject
    {

        /// <summary>
        /// Parameterised initialiser for the main object properties that also initialises the lock object
        /// </summary>
        /// <param name="DeviceObjectParm"></param>
        /// <param name="AllowConnectedSetFalseParm"></param>
        /// <param name="AllowSetConnectedTrueParm"></param>
        public ActiveObject(bool AllowConnectedSetFalseParm, bool AllowSetConnectedTrueParm, bool AllowConcurrentAccessParm, string ConfiguredDeviceKeyParm)
        {
            CommandLock = new object(); // Create a lock object
            InitialisedOk = false; // Initialise flag and error message
            InitialisationErrorMessage = "ASCOM Remote ActiveObject error message - something went wrong during device initialisation but the error message was not recorded for an unknown reason.";
            AllowConnectedSetFalse = AllowConnectedSetFalseParm;
            AllowConnectedSetTrue = AllowSetConnectedTrueParm;
            AllowConcurrentAccess = AllowConcurrentAccessParm;
            ConfiguredDeviceKey = ConfiguredDeviceKeyParm;
        }

        /// <summary>
        /// Key of the corresponding entry in ConfiguredDevices to allow tieback to the main configuration information
        /// </summary>
        public string ConfiguredDeviceKey { get; set; }

        /// <summary>
        /// The device COM object
        /// </summary>
        public dynamic DeviceObject { get; set; }

        /// <summary>
        /// Flag indicating whether the user is allowed to set the device's Connected property to False
        /// </summary>
        public bool AllowConnectedSetFalse { get; set; }

        /// <summary>
        /// Flag indicating whether the user is allowed to set the device's Connected property to True
        /// </summary>
        public bool AllowConnectedSetTrue { get; set; }

        /// <summary>
        /// Flag indicating whether the driver can handle concurrent calls
        /// </summary>
        public bool AllowConcurrentAccess { get; set; }

        /// <summary>
        /// Lock object to ensure that only one command at a time is sent to the device
        /// </summary>
        public object CommandLock { get; }

        /// <summary>
        /// Host form for the device when devices are set to run on independent threads
        /// </summary>
        public DriverHostForm DriverHostForm { get; set; }

        /// <summary>
        /// Flag indicating whether the device was created and the Connected property set True without error
        /// </summary>
        public bool InitialisedOk { get; set; }

        /// <summary>
        /// Error message that will be returned if the user tries to call a method on a device that did not initialise correctly.
        /// </summary>
        public string InitialisationErrorMessage { get; set; }

        /// <summary>
        /// If the device is a Camera, points to the last image array value returned, otherwise null
        /// </summary>
        public object LastImageArray { get; set; }
    }
}
