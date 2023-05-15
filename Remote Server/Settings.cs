using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    internal class Settings
    {
        internal static bool RunAs64Bit { get; set; } = false;

        internal static void GetSettings()
        {
            RunAs64Bit = true;
        }
        internal static void SaveSettings()
        {

        }


    }
}
