using System;
using System.Globalization;
using Microsoft.Win32;

namespace ASCOM.Remote
{
    class Configuration : IDisposable
    {
        private bool LOG_CONFIGURATION_CALLS = false; // Stored as a variable rather than a const to avoid compiler warnings about unreachable code

        private RegistryKey hiveKey, baseRegistryKey;

        public Configuration()
        {

            try
            {
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "Configuration New", "About to create base key");
                hiveKey = RegistryKey.OpenBaseKey(SharedConstants.ASCOM_REMOTE_CONFIGURATION_HIVE, RegistryView.Default);
                baseRegistryKey = hiveKey.CreateSubKey(SharedConstants.ASCOM_REMOTE_CONFIGURATION_KEY);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "Configuration New", "Created base key: " + baseRegistryKey.Name);
            }
            catch (Exception ex)
            {
                ServerForm.LogException(0, 0, 0, "Configuration New", ex.ToString());
            }
        }

        public T GetValue<T>(string KeyName, string SubKey, T DefaultValue)
        {
            if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", string.Format("Getting {0} value '{1}' in subkey '{2}', default: '{3}'", typeof(T).Name, KeyName, SubKey, DefaultValue.ToString()));
            if (typeof(T) == typeof(bool))
            {
                string registryValue;
                if (SubKey == "")
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey is empty so getting value directly");
                    registryValue = (string)baseRegistryKey.GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }
                else
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey has a value so using it...");
                    registryValue = (string)baseRegistryKey.CreateSubKey(SubKey).GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }

                if (registryValue == null)
                {
                    SetValue<T>(KeyName, SubKey, DefaultValue);
                    registryValue = DefaultValue.ToString();
                }
                bool RetVal = Convert.ToBoolean(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", string.Format("Retrieved {0} = {1}", KeyName, RetVal.ToString()));
                return (T)((object)RetVal);
            }

            if (typeof(T) == typeof(string))
            {
                string RetVal;
                if (SubKey == "")
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey is empty so getting value directly");
                    RetVal = (string)baseRegistryKey.GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + RetVal);
                }
                else
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey has a value so using it...");
                    RetVal = (string)baseRegistryKey.CreateSubKey(SubKey).GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + RetVal);
                }

                if (RetVal == null)
                {
                    SetValue<T>(KeyName, SubKey, DefaultValue);
                    RetVal = DefaultValue.ToString();
                }
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", string.Format("Retrieved {0} = {1}", KeyName, RetVal.ToString()));
                return (T)((object)RetVal);
            }

            if (typeof(T) == typeof(decimal))
            {
                string registryValue;
                if (SubKey == "")
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey is empty so getting value directly");
                    registryValue = (string)baseRegistryKey.GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }
                else
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey has a value so using it...");
                    registryValue = (string)baseRegistryKey.CreateSubKey(SubKey).GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }

                if (registryValue == null)
                {
                    SetValue<T>(KeyName, SubKey, DefaultValue);
                    registryValue = DefaultValue.ToString();
                }
                decimal RetVal = Convert.ToDecimal(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", string.Format("Retrieved {0} = {1}", KeyName, RetVal.ToString()));
                return (T)((object)RetVal);
            }

            if (typeof(T) == typeof(Int32))
            {
                string registryValue;
                if (SubKey == "")
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey is empty so getting value directly");
                    registryValue = (string)baseRegistryKey.GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }
                else
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "SubKey has a value so using it...");
                    registryValue = (string)baseRegistryKey.CreateSubKey(SubKey).GetValue(KeyName);
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", "Value retrieved OK: " + registryValue);
                }

                if (registryValue == null)
                {
                    SetValue<T>(KeyName, SubKey, DefaultValue);
                    registryValue = DefaultValue.ToString();
                }
                Int32 RetVal = Convert.ToInt32(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", string.Format("Retrieved {0} = {1}", KeyName, RetVal.ToString()));
                return (T)((object)RetVal);
            }

            throw new DriverException("GetValue: Unknown type: " + typeof(T).Name);
        }

        public void SetValue<T>(string KeyName, string SubKey, T Value)
        {
            if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "SetValue", string.Format("Setting {0} value '{1}' in subkey '{2}' to: '{3}'", typeof(T).Name, KeyName, SubKey, Value.ToString()));

            if (SubKey == "") baseRegistryKey.SetValue(KeyName, Value.ToString());
            else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, Value.ToString());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (baseRegistryKey != null) baseRegistryKey.Dispose();
                    if (hiveKey != null) hiveKey.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put clean up code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
