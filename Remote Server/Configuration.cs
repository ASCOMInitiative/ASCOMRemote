using System;
using System.Globalization;
using Microsoft.Win32;

namespace ASCOM.Remote
{
    class Configuration : IDisposable
    {
        private readonly bool LOG_CONFIGURATION_CALLS = false; // Stored as a variable rather than a const to avoid compiler warnings about unreachable code

        private readonly RegistryKey hiveKey, baseRegistryKey;

        public static void Reset()
        {
            try
            {
                RegistryKey hiveKey = RegistryKey.OpenBaseKey(SharedConstants.ASCOM_REMOTE_CONFIGURATION_HIVE, RegistryView.Default);
                hiveKey.DeleteSubKeyTree(SharedConstants.ASCOM_REMOTE_CONFIGURATION_KEY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception reseting configuration:\r\n{ex}");
            }
        }

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
            if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", $"Getting {typeof(T).Name} value '{KeyName}' in subkey '{SubKey}', default: '{DefaultValue}'");

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
                    SetValueInvariant<T>(KeyName, SubKey, DefaultValue);
                    bool defaultValue = Convert.ToBoolean(DefaultValue);
                    registryValue = defaultValue.ToString(CultureInfo.InvariantCulture);
                }

                bool RetVal = Convert.ToBoolean(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", $"Retrieved {KeyName} = {RetVal}");
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
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", $"Retrieved {KeyName} = {RetVal}");
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
                    SetValueInvariant<T>(KeyName, SubKey, DefaultValue);
                    decimal defaultValue = Convert.ToDecimal(DefaultValue);
                    registryValue = defaultValue.ToString(CultureInfo.InvariantCulture);
                }

                decimal RetVal = Convert.ToDecimal(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", $"Retrieved {KeyName} = {RetVal}");
                return (T)((object)RetVal);
            }

            if (typeof(T) == typeof(DateTime))
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
                    SetValueInvariant<T>(KeyName, SubKey, DefaultValue);
                    return DefaultValue;
                }

                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue DateTime", $"String value prior to Convert: {registryValue}");

                if (DateTime.TryParse(registryValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime RetVal))
                {
                    // The string parsed OK so return the parsed value;
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue DateTime", $"Retrieved {KeyName} = {RetVal}");
                    return (T)((object)RetVal);
                }
                else // If the string fails to parse, overwrite with the default value and return this
                {
                    if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue DateTime", $"Failed to parse registry value, persisting and returning the default value: {DefaultValue}");
                    SetValueInvariant<T>(KeyName, SubKey, DefaultValue);
                    return DefaultValue;
                }
            }

            if ((typeof(T) == typeof(Int32)) | (typeof(T) == typeof(int)))
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
                    SetValueInvariant<T>(KeyName, SubKey, DefaultValue);
                    int defaultValue = Convert.ToInt32(DefaultValue);
                    registryValue = defaultValue.ToString(CultureInfo.InvariantCulture);
                }

                int RetVal = Convert.ToInt32(registryValue, CultureInfo.InvariantCulture);
                if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "GetValue", $"Retrieved {KeyName} = {RetVal}");
                return (T)((object)RetVal);
            }

            throw new DriverException("GetValue: Unknown type: " + typeof(T).Name);
        }

        public void SetValue<T>(string KeyName, string SubKey, T Value)
        {
            if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "SetValue", $"Setting {typeof(T).Name} value '{KeyName}' in subkey '{SubKey}' to: '{Value}'");

            if (SubKey == "") baseRegistryKey.SetValue(KeyName, Value.ToString());
            else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, Value.ToString());
        }

        public void SetValueInvariant<T>(string KeyName, string SubKey, T Value)
        {
            if (LOG_CONFIGURATION_CALLS) ServerForm.LogMessage(0, 0, 0, "SetValue DateTime", $"Setting {typeof(T).Name} value '{KeyName}' in subkey '{SubKey}' to: '{Value}'");

            if ((typeof(T) == typeof(Int32)) | (typeof(T) == typeof(int)))
            {
                int intValue = Convert.ToInt32(Value);
                if (SubKey == "") baseRegistryKey.SetValue(KeyName, intValue.ToString(CultureInfo.InvariantCulture));
                else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, intValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (typeof(T) == typeof(bool))
            {
                bool boolValue = Convert.ToBoolean(Value);
                if (SubKey == "") baseRegistryKey.SetValue(KeyName, boolValue.ToString(CultureInfo.InvariantCulture));
                else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, boolValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (typeof(T) == typeof(decimal))
            {
                decimal decimalValue = Convert.ToDecimal(Value);
                if (SubKey == "") baseRegistryKey.SetValue(KeyName, decimalValue.ToString(CultureInfo.InvariantCulture));
                else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, decimalValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (typeof(T) == typeof(DateTime))
            {
                DateTime dateTimeValue = Convert.ToDateTime(Value);
                if (SubKey == "") baseRegistryKey.SetValue(KeyName, dateTimeValue.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                else baseRegistryKey.CreateSubKey(SubKey).SetValue(KeyName, dateTimeValue.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                return;
            }

            throw new DriverException("SetValueInvariant: Unknown type: " + typeof(T).Name);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    baseRegistryKey?.Dispose();
                    hiveKey?.Dispose();
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
