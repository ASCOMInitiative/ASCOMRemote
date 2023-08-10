using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ASCOM.Remote
{
    [DefaultMember("Settings")]
    internal class ConfigurationManager : IDisposable
    {
        private const string FOLDER_NAME = "ASCOM Remote"; // Folder name underneath the local application data folder
        private const string SETTINGS_FILENAME = "remote.settings"; // Settings file name

        private TraceLoggerPlus TL;
        private Settings settings;
        private bool disposedValue;
        readonly int settingsFileVersion;
        private readonly JsonDocument appSettingsDocument = null;


        #region Initialiser and Dispose

        /// <summary>
        /// Create a Configuration management instance and load the current settings
        /// </summary>
        /// <param name="logger">Data logger instance.</param>
        public ConfigurationManager(TraceLoggerPlus logger)
        {
            TL = logger;

            try
            {
                // Create a new settings file with default values in case the supplied file cannot be used
                settings = new();

                // Get the full settings file name including path
                string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FOLDER_NAME);
                SettingsFileName = Path.Combine(folderName, SETTINGS_FILENAME);
                TL?.LogMessage("ConfigurationManager", $"Settings folder: {folderName}, Settings file: {SettingsFileName}");

                // Load the values in the settings file if it exists
                if (File.Exists(SettingsFileName)) // Settings file exists
                {
                    // Read the file contents into a string
                    TL?.LogMessage("ConfigurationManager", "File exists, about to read it...");
                    string serialisedSettings = File.ReadAllText(SettingsFileName);
                    TL?.LogMessage("ConfigurationManager", $"Serialised settings: \r\n{serialisedSettings}");

                    // Make a basic check to see if this file is a beta / pre-release version that doesn't have a version number. If so replace with a new version
                    if (!serialisedSettings.Contains("\"SettingsCompatibilityVersion\":")) // No compatibility version found so assume that this is a corrupt settings file
                    {
                        // Persist the default settings values
                        try
                        {
                            // Rename the current settings file to preserve it
                            string badVersionSettingsFileName = $"{SettingsFileName}.bad";
                            File.Delete(badVersionSettingsFileName);
                            File.Move(SettingsFileName, $"{badVersionSettingsFileName}");

                            // Persist the default settings values
                            settings = new();
                            PersistSettings(settings);

                            Status = $"A corrupt settings file was found.\r\n\r\nApplication settings have been reset to defaults and the original settings file has been renamed to {badVersionSettingsFileName}.";
                        }
                        catch (Exception ex2)
                        {
                            TL?.LogMessage("ConfigurationManager", $"A corrupt settings file found but an error occurred when saving new Remote Server settings: {ex2}");
                            Status = $"$\"A corrupt settings file was found but an error occurred when saving new Remote Server settings: {ex2.Message}.";
                        }
                    }
                    else // File does have a compatibility version so read in the settings from the file
                    {
                        TL?.LogMessage("ConfigurationManager", $"Found compatibility version element...");
                        // Try to read in the settings version number from the settings file
                        try
                        {
                            TL?.LogMessage("ConfigurationManager", $"About to parse settings string");
                            appSettingsDocument = JsonDocument.Parse(serialisedSettings, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
                            TL?.LogMessage("ConfigurationManager", $"About to get settings version");
                            settingsFileVersion = appSettingsDocument.RootElement.GetProperty("SettingsCompatibilityVersion").GetInt32();
                            TL?.LogMessage("ConfigurationManager", $"Found settings version: {settingsFileVersion}");

                            // Handle different file versions
                            switch (settingsFileVersion)
                            {
                                // File version 1 - first production release
                                case 1:

                                    try
                                    {
                                        // Set de-serialisation options
                                        JsonSerializerOptions options = new()
                                        {
                                            PropertyNameCaseInsensitive = true // Ignore incorrect element name casing
                                        };
                                        options.Converters.Add(new JsonStringEnumConverter()); // For increased resilience, accept both string member names and integer member values as valid for enum elements.

                                        // De-serialise the settings string into a Settings object
                                        settings = JsonSerializer.Deserialize<Settings>(serialisedSettings, options);

                                        // Test whether the retrieved settings match the requirements of this version of the Remote Server
                                        if (settings.SettingsCompatibilityVersion == Settings.SETTINGS_COMPATIBILTY_VERSION) // Version numbers match so all is well
                                        {
                                            Status = $"Settings read OK.";
                                        }
                                        else // Version numbers don't match so reset to defaults
                                        {
                                            int originalSettingsCompatibilityVersion = 0;
                                            try
                                            {
                                                originalSettingsCompatibilityVersion = settings.SettingsCompatibilityVersion;

                                                // Rename the current settings file to preserve it
                                                string badVersionSettingsFileName = $"{SettingsFileName}.badversion";
                                                File.Delete(badVersionSettingsFileName);
                                                File.Move(SettingsFileName, $"{badVersionSettingsFileName}");

                                                // Persist the default settings values
                                                settings = new();
                                                PersistSettings(settings);

                                                Status = $"The current settings version: {originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION}. Application settings have been reset to default values and the original settings file renamed to {badVersionSettingsFileName}.";
                                            }
                                            catch (Exception ex2)
                                            {
                                                TL?.LogMessage("ConfigurationManager", $"Error persisting new Remote Server settings file: {ex2}");
                                                Status = $"The current settings version:{originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION} but the new settings could not be saved: {ex2.Message}.";
                                            }
                                        }
                                    }
                                    catch (JsonException ex1)
                                    {
                                        // There was an exception when parsing the settings file so report it and set default values
                                        TL?.LogMessage("ConfigurationManager", $"Error de-serialising Remote Server settings file: {ex1}");
                                        Status = $"There was an error de-serialising the settings file and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to save new values.\r\n\r\nJSON parser error message:\r\n{ex1.Message}";
                                    }
                                    catch (Exception ex1)
                                    {
                                        TL?.LogMessage("ConfigurationManager", ex1.ToString());
                                        Status = $"Exception reading the settings file, default values are in effect.";
                                    }
                                    break;

                                // Handle unknown settings version numbers
                                default:

                                    // Persist default settings values because the file version is unknown and the file may be corrupt
                                    try
                                    {
                                        // Rename the current settings file to preserve it
                                        string badVersionSettingsFileName = $"{SettingsFileName}.unknownversion";
                                        File.Delete(badVersionSettingsFileName);
                                        File.Move(SettingsFileName, $"{badVersionSettingsFileName}");

                                        // Persist the default settings values
                                        settings = new();
                                        PersistSettings(settings);

                                        Status = $"An unsupported settings version was found: {settingsFileVersion}. Settings have been reset to defaults and the original settings file has been renamed to {badVersionSettingsFileName}.";
                                    }
                                    catch (Exception ex2)
                                    {
                                        TL?.LogMessage("ConfigurationManager", $"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new Remote Server settings: {ex2}");
                                        Status = $"$\"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new Remote Server settings: {ex2.Message}.";
                                    }
                                    break;
                            }
                        }
                        catch (JsonException ex)
                        {
                            // There was an exception when parsing the settings file so report it and use default values
                            TL?.LogMessage("ConfigurationManager", $"Error getting settings file version from settings file: {ex}");
                            Status = $"An error occurred when reading the settings file version and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to create a new settings file.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                        }
                        catch (Exception ex)
                        {
                            TL?.LogMessage("ConfigurationManager", $"Exception parsing the settings file: {ex}");
                            Status = $"Exception parsing the settings file: {ex.Message}";
                        }
                        finally
                        {
                            appSettingsDocument?.Dispose();
                        }
                    }
                }
                else // Settings file does not exist
                {
                    TL.LogMessage("ConfigurationManager", $"Configuration file does not exist, initialising new file: {SettingsFileName}");
                    PersistSettings(settings);
                    Status = $"First time use - configuration set to default values.";
                }
            }
            catch (Exception ex)
            {
                TL?.LogMessage("ConfigurationManager", ex.ToString());
                Status = $"Unexpected exception reading the settings file, default values are in use.";
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TL = null;
                    settings = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put clean-up code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public methods

        public void Reset()
        {
            try
            {
                settings = new();
                PersistSettings(settings);
                Status = $"Settings reset at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                TL?.LogMessage("Reset", $"Exception during Reset: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Persist current settings
        /// </summary>
        public void Save()
        {
            TL?.LogMessage("Save", "Saving settings to settings file");
            PersistSettings(settings);
            Status = $"Settings saved at {DateTime.Now:HH:mm:ss}.";
        }

        public Settings Settings
        {
            get { return settings; }
        }

        public string SettingsFileName { get; private set; }

        /// <summary>
        /// Text message describing any issues found when validating the settings
        /// </summary>
        public string Status { get; private set; }

        #endregion

        #region Support code

        private void PersistSettings(Settings settingsToPersist)
        {
            try
            {
                if (settingsToPersist is null)
                {
                    throw new ArgumentNullException(nameof(settingsToPersist));
                }

                // Set the version number of this settings file
                settingsToPersist.SettingsCompatibilityVersion = Settings.SETTINGS_COMPATIBILTY_VERSION;

                TL?.LogMessage("PersistSettings", $"Settings file: {SettingsFileName}");

                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                string serialisedSettings = JsonSerializer.Serialize<Settings>(settingsToPersist, options);

                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName));
                File.WriteAllText(SettingsFileName, serialisedSettings);
            }
            catch (Exception ex)
            {
                TL.LogMessage("PersistSettings", ex.ToString());
            }

        }

        //internal void RaiseUiHasChangedEvent()
        //{
        //    if (UiHasChanged is not null)
        //    {
        //        EventArgs args = new();
        //        TL?.LogMessage("RaiseUiHasChangedEvent", "About to call UI has changed event handler");
        //        UiHasChanged(this, args);
        //        TL?.LogMessage("RaiseUiHasChangedEvent", "Returned from UI has changed event handler");
        //    }
        //}

        #endregion

    }
}
