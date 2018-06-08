using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
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
}
