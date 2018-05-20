using System;
using System.Runtime.InteropServices;

namespace ASCOM.Remote
{
    /// <summary>
    /// ASCOM Rotator Driver
    /// </summary>
    [Guid("93D0DEED-FC1A-4770-88B0-CB4F6CF5E6B9")]
    [ProgId(DRIVER_PROGID)]
    [ServedClassName(DRIVER_DISPLAY_NAME)]
    [ClassInterface(ClassInterfaceType.None)]
    public class Rotator : RotatorBaseClass
    {
        #region Variables and Constants

        // Constants unique to this particular driver
        private const string DRIVER_NUMBER = "1";
        private const string DEVICE_TYPE = "Rotator";

        // Derrived constants common to all drivers provided by the local server
        private const string DRIVER_DISPLAY_NAME = SharedConstants.DRIVER_DISPLAY_NAME + " " + DRIVER_NUMBER; // Driver description that displays in the ASCOM Chooser.
        private const string DRIVER_PROGID = SharedConstants.DRIVER_PROGID_BASE + DRIVER_NUMBER + "." + DEVICE_TYPE; // This driver's ProgID

        #endregion

        #region Initialiser

        /// <summary>
        /// Initializes a new instance of the device class using the supplied  driver number.
        /// Must be public for COM registration.
        /// </summary>
        public Rotator() : base(DRIVER_NUMBER, DRIVER_DISPLAY_NAME, DRIVER_PROGID)
        {
        }

        #endregion

    }
}