using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    internal class Settings
    {
        // Current version number for this settings class. Only needs to be incremented when there are breaking changes!
        // For example this can be left at its current level when adding new settings that have usable default values.

        // This value is set when values are actually persisted in ConformConfiguration.PersistSettings in order not to overwrite the value that is retrieved from the current settings file when it is read.
        internal const int SETTINGS_COMPATIBILTY_VERSION = 1;

        public int SettingsCompatibilityVersion { get; set; } = SETTINGS_COMPATIBILTY_VERSION; // Default is zero so that versions prior to introduction of the settings compatibility version number can be detected.

        public bool RunAs64Bit { get; set; } = false;
    }
}
