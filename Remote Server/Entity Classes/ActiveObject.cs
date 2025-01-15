using System;
namespace ASCOM.Remote
{
    /// <summary>
    /// Class to hold information about an ASCOM Remote served device
    /// </summary>
    /// <remarks>
    /// Parameterised initialiser for the main object properties that also initialises the lock object
    /// </remarks>
    /// <param name="DeviceObjectParm"></param>
    /// <param name="AllowConnectedSetFalseParm"></param>
    /// <param name="AllowSetConnectedTrueParm"></param>
    public class ActiveObject(bool AllowConnectedSetFalseParm, bool AllowSetConnectedTrueParm, bool AllowConcurrentAccessParm, string ConfiguredDeviceKeyParm)
    {
        private int? interfaceVersion;

        /// <summary>
        /// Key of the corresponding entry in ConfiguredDevices to allow tieback to the main configuration information
        /// </summary>
        public string ConfiguredDeviceKey { get; set; } = ConfiguredDeviceKeyParm;

        /// <summary>
        /// The device COM object
        /// </summary>
        public dynamic DeviceObject { get; set; }

        /// <summary>
        /// Flag indicating whether the user is allowed to set the device's Connected property to False
        /// </summary>
        public bool AllowConnectedSetFalse { get; set; } = AllowConnectedSetFalseParm;

        /// <summary>
        /// Flag indicating whether the user is allowed to set the device's Connected property to True
        /// </summary>
        public bool AllowConnectedSetTrue { get; set; } = AllowSetConnectedTrueParm;

        /// <summary>
        /// Flag indicating whether the driver can handle concurrent calls
        /// </summary>
        public bool AllowConcurrentAccess { get; set; } = AllowConcurrentAccessParm;

        /// <summary>
        /// Lock object to ensure that only one command at a time is sent to the device
        /// </summary>
        public object CommandLock { get; } = new object(); // Create a lock object

        /// <summary>
        /// Host form for the device when devices are set to run on independent threads
        /// </summary>
        public DriverHostForm DriverHostForm { get; set; }

        /// <summary>
        /// Flag indicating whether the device was created and the Connected property set True without error
        /// </summary>
        public bool InitialisedOk { get; set; } = false; // Initialise flag and error message

        /// <summary>
        /// Error message that will be returned if the user tries to call a method on a device that did not initialise correctly.
        /// </summary>
        public string InitialisationErrorMessage { get; set; } = "ASCOM Remote ActiveObject error message - something went wrong during device initialisation but the error message was not recorded for an unknown reason.";

        /// <summary>
        /// If the device is a Camera, points to the last image array value returned, otherwise null
        /// </summary>
        public object LastImageArray { get; set; }

        /// <summary>
        /// The device's interface version
        /// </summary>
        public int InterfaceVersion
        {
            get
            {
                // Check whether we have already cached this device's interface version
                if (interfaceVersion.HasValue) // Cached value available so return it.
                    return interfaceVersion.Value;

                // No cached value so query it from the device
                try
                {
                    interfaceVersion = DeviceObject.Interfaceversion;
                }
                catch // Something went wrong, so most likely this is an old simulator without an InterfceVersion property - Set the property to 1.
                {
                    interfaceVersion = 1;
                }

                // Return the interface version.
                return interfaceVersion.Value;
            }
        }
    }
}
