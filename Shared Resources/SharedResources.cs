//
// ================
// Shared Resources
// ================
//
// This class is a container for all shared resources that may be needed
// by the drivers served by the Local Server. 
//
// NOTES:
//
//	* ALL DECLARATIONS MUST BE STATIC HERE!! INSTANCES OF THIS CLASS MUST NEVER BE CREATED!
//
// Written by:	Bob Denny	29-May-2007
// Modified by Chris Rowland and Peter Simpson to hamdle multiple hardware devices March 2011
//
using System;
using System.Collections.Generic;

namespace ASCOM.Remote
{
    /// <summary>
    /// The resources shared by all drivers and devices, in this example it's a serial port with a shared SendMessage method
    /// an idea for locking the message and handling connecting is given.
    /// In reality extensive changes will probably be needed.
    /// Multiple drivers means that several applications connect to the same hardware device, aka a hub.
    /// Multiple devices means that there are more than one instance of the hardware, such as two focusers.
    /// In this case there needs to be multiple instances of the hardware connector, each with it's own connection count.
    /// </summary>
    public static class SharedResources
    {
        // object used for locking to prevent multiple drivers accessing common code at the same time
        private static readonly object lockObject = new object();

        //
        // Public access to shared resources
        //

        #region Multi Driver handling
        // this section illustrates how multiple drivers could be handled,
        // it's for drivers where multiple connections to the hardware can be made and ensures that the
        // hardware is only disconnected from when all the connected devices have disconnected.

        // It is NOT a complete solution!  This is to give ideas of what can - or should be done.
        //
        // An alternative would be to move the hardware control here, handle connecting and disconnecting,
        // and provide the device with a suitable connection to the hardware.
        //
        /// <summary>
        /// dictionary carrying device connections.
        /// The Key is the connection number that identifies the device, it could be the COM port name,
        /// USB ID or IP Address, the Value is the DeviceHardware class
        /// </summary>
        private static Dictionary<string, DeviceHardware> connectedDevices = new Dictionary<string, DeviceHardware>();

        /// <summary>
        /// This is called in the driver Connect(true) property,
        /// it add the device id to the list of devices if it's not there and increments the device count.
        /// </summary>
        /// <param name="deviceId"></param>
        public static void Connect(string deviceId)
        {
            lock (lockObject)
            {
                if (!connectedDevices.ContainsKey(deviceId))
                    connectedDevices.Add(deviceId, new DeviceHardware());
                connectedDevices[deviceId].Count++;       // increment the value
            }
        }

        public static void Disconnect(string deviceId)
        {
            lock (lockObject)
            {
                if (connectedDevices.ContainsKey(deviceId))
                {
                    connectedDevices[deviceId].Count--;
                    if (connectedDevices[deviceId].Count <= 0)
                        connectedDevices.Remove(deviceId);
                }
            }
        }

        public static bool IsConnected(string deviceId)
        {
            if (connectedDevices.ContainsKey(deviceId))
                return (connectedDevices[deviceId].Count > 0);
            else
                return false;
        }

        #endregion

    }

    /// <summary>
    /// Skeleton of a hardware class, all this does is hold a count of the connections,
    /// in reality extra code will be needed to handle the hardware in some way
    /// </summary>
    public class DeviceHardware
    {
        internal int Count { set; get; }

        internal DeviceHardware()
        {
            Count = 0;
        }
    }
    public static class ExtensionMethods
    {
        /// <summary>
        /// Append a byte array to an existing array
        /// </summary>
        /// <param name="first">First array</param>
        /// <param name="second">Second array</param>
        /// <returns>Concatenated array of byte</returns>
        public static byte[] Append(this byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }

    public static class Library
    {
        /// <summary>
        /// Convert 2D 16bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">16bit integer array to be converted</param>
        /// <returns>2D double array containing the integer values</returns>
        public static double[,] IntToDouble(short[,] inputArray)
        {
            double[,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1)];

            for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                {
                    outputArray[i, j] = inputArray[i, j];
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 2D 32bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>2D double array containing the integer values</returns>
        public static double[,] IntToDouble(int[,] inputArray)
        {
            double[,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1)];

            for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                {
                    outputArray[i, j] = inputArray[i, j];
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D 16bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">16bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static double[,,] IntToDouble(short[,,] inputArray)
        {
            double[,,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1), inputArray.GetLength(2)];

            for (int k = 0; k <= inputArray.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                    {
                        outputArray[i, j, k] = inputArray[i, j, k];
                    }
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D 32bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static double[,,] IntToDouble(int[,,] inputArray)
        {
            double[,,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1), inputArray.GetLength(2)];

            for (int k = 0; k <= inputArray.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                    {
                        outputArray[i, j, k] = inputArray[i, j, k];
                    }
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D 32bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static double[,] Array2DToDouble(Array inputArray)
        {
            double[,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1)];

            for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                {
                    outputArray[i, j] = Convert.ToDouble(inputArray.GetValue(i, j));
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static double[,,] Array3DToDouble(Array inputArray)
        {
            double[,,] outputArray = new double[inputArray.GetLength(0), inputArray.GetLength(1), inputArray.GetLength(2)];

            for (int k = 0; k <= inputArray.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                    {
                        outputArray[i, j, k] = Convert.ToDouble(inputArray.GetValue(i, j, k));
                    }
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D 32bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static int[,] Array2DToInt(Array inputArray)
        {
            int[,] outputArray = new int[inputArray.GetLength(0), inputArray.GetLength(1)];

            for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                {
                    outputArray[i, j] = Convert.ToInt32(inputArray.GetValue(i, j));
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static int[,,] Array3DToInt(Array inputArray)
        {
            int[,,] outputArray = new int[inputArray.GetLength(0), inputArray.GetLength(1), inputArray.GetLength(2)];

            for (int k = 0; k <= inputArray.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                    {
                        outputArray[i, j, k] = Convert.ToInt32(inputArray.GetValue(i, j, k));
                    }
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D 32bit Integer array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static short[,] Array2DToShort(Array inputArray)
        {
            short[,] outputArray = new short[inputArray.GetLength(0), inputArray.GetLength(1)];

            for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
            {
                for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                {
                    outputArray[i, j] = Convert.ToInt16(inputArray.GetValue(i, j));
                }
            }
            return outputArray;
        }

        /// <summary>
        /// Convert 3D array to double array
        /// </summary>
        /// <param name="inputArray">32bit integer array to be converted</param>
        /// <returns>3D double array containing the integer values</returns>
        public static short[,,] Array3DToShort(Array inputArray)
        {
            short[,,] outputArray = new short[inputArray.GetLength(0), inputArray.GetLength(1), inputArray.GetLength(2)];

            for (int k = 0; k <= inputArray.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= inputArray.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= inputArray.GetUpperBound(0); i++)
                    {
                        outputArray[i, j, k] = Convert.ToInt16(inputArray.GetValue(i, j, k));
                    }
                }
            }
            return outputArray;
        }




    }
}
