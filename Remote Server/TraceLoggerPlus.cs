using ASCOM.Common.Interfaces;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ASCOM.Remote
{
    public class TraceLoggerPlus : ITraceLogger
    {
        #region Constants and Variables
        private const string ID_FORMAT_STRING = "  {0,6} {1,6} {2,6}";
        private const string MESSAGE_FORMAT_STRING_NO_IP_ADDRESS = "{0,-22} {1}";
        private const string MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS = "{0,-22} {1,-22} {2}";

        // Configuration constants
        private const int IDENTIFIER_WIDTH_DEFAULT = 25;
        private const int MUTEX_WAIT_TIME = 5000;
        private const int MAXIMUM_UNIQUE_SUFFIX_ATTEMPTS = 20;

        // Path and file name constants for auto generated paths and file names
        private const string AUTO_FILE_NAME_TEMPLATE = "ASCOM.{0}.{1:HHmm.ssfff}{2}.txt"; // Auto generated file name template
        private const string AUTO_PATH_BASE_DIRECTORY = "ASCOM"; // Primary logging directory off the users's Documents (Windows) or HOME directory (Linux)
        private const string AUTO_PATH_WINDOWS_SYSTEM_USER_BASE_DIRECTORY = @"ASCOM\SystemLogs"; // Primary logging directory for the System account
        private const string AUTO_PATH_WINDOWS_DIRECTORY_TEMPLATE = "Logs {0:yyyy-MM-dd}"; // Sub directory template on Windows 

        // Default value constants
        private const bool USE_UTC_DEFAULT = false;
        private const bool AUTO_GENERATE_FILENAME_DEFAULT = true;
        private const bool AUTO_GENERATE_FILEPATH_DEFAULT = true;
        private const bool RESPECT_CRLF_DEFAULT = true;

        // Property backing variables
        private readonly string logFileType;
        private int identifierWidthValue;

        // Global fields
        private StreamWriter logFileStream;
        private Mutex loggerMutex;
        private readonly bool autoGenerateFileName;
        private readonly bool autoGenerateFilePath;
        private bool traceLoggerHasBeenDisposed;
        private string mutexName;

        #endregion

        #region Initialise and Dispose

        public TraceLoggerPlus(string fileName, string logName) : this(fileName, "", logName, false)
        {
            IpAddressTraceState = true;
        }

        /// <summary>
        /// Create a new TraceLogger instance with the given filename and path
        /// </summary>
        /// <param name="logFileName">Name of the log file (without path) or null / empty string to use automatic file naming.</param>
        /// <param name="logFilePath">Fully qualified path to the log file directory or null / empty string to use an automatically generated path.</param>
        /// <param name="logFileType">A short name to identify the contents of the log (only used in automatic file names).</param>
        /// <param name="enabled">Initial state of the trace logger - Enabled or Disabled.</param>
        /// <remarks>Automatically generated directory names will be of the form: <c>"Documents\ASCOM\Logs {CurrentDate:yyyymmdd}"</c> on Windows and <c>"HOME/ASCOM/Logs{CurrentDate:yyyymmdd}"</c> on Linux
        /// Automatically generated file names will be of the form: <c>"ASCOM.{LogFileType}.{CurrentTime:HHmm.ssfff}{1 or 2 Digits, usually 0}.txt"</c>.</remarks>
        public TraceLoggerPlus(string logFileName, string logFilePath, string logFileType, bool enabled)
        {
            if (string.IsNullOrEmpty(logFileType)) throw new InvalidValueException("TraceLogger Initialisation - Supplied log file type is null or empty");

            IpAddressTraceState = true;

            CommonInitialisation();

            LogFileName = logFileName;
            LogFilePath = logFilePath;
            this.logFileType = logFileType;
            Enabled = enabled;

            autoGenerateFileName = string.IsNullOrEmpty(LogFileName); // Flag auto file name generation if no file name is supplied
            autoGenerateFilePath = string.IsNullOrEmpty(LogFilePath); // Flag auto file path use if no path is supplied
        }

        /// <summary>
        /// Create a new TraceLogger instance with automatic naming incorporating the supplied log file type
        /// </summary>
        /// <param name="logFileType">A short name to identify the contents of the log.</param>
        /// <param name="enabled">Initial state of the trace logger - Enabled or Disabled.</param>
        /// <remarks>Automatically generated directory names will be of the form: <c>"Documents\ASCOM\Logs {CurrentDate:yyyymmdd}"</c> on Windows and <c>"HOME/ASCOM/Logs{CurrentDate:yyyymmdd}"</c> on Linux
        /// Automatically generated file names will be of the form: <c>"ASCOM.{LogFileType}.{CurrentTime:HHmm.ssfff}{1 or 2 Digits, usually 0}.txt"</c>.</remarks>
        public TraceLoggerPlus(string logFileType, bool enabled)
        {
            if (string.IsNullOrEmpty(logFileType)) throw new InvalidValueException("TraceLogger Initialisation - Supplied log file type is null or empty");

            CommonInitialisation();

            LogFileName = "";
            LogFilePath = "";
            this.logFileType = logFileType;
            Enabled = enabled;

            autoGenerateFileName = AUTO_GENERATE_FILENAME_DEFAULT;
            autoGenerateFilePath = AUTO_GENERATE_FILEPATH_DEFAULT;
        }

        /// <summary>
        /// Common code shared by all initialiser overloads
        /// </summary>
        private void CommonInitialisation()
        {
            traceLoggerHasBeenDisposed = false;
            identifierWidthValue = IDENTIFIER_WIDTH_DEFAULT;

            mutexName = Guid.NewGuid().ToString().ToUpper();
            loggerMutex = new Mutex(false, mutexName);

            UseUtcTime = USE_UTC_DEFAULT;
            RespectCrLf = RESPECT_CRLF_DEFAULT;
        }

        /// IDisposable
        /// <summary>
        /// Disposes of the TraceLogger object
        /// </summary>
        /// <param name="disposing">True if being disposed by the application, False if disposed by the finaliser.</param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (traceLoggerHasBeenDisposed) return;

            if (disposing)
            {
                if (logFileStream != null)
                {
                    try
                    {
                        logFileStream.Flush();
                    }
                    catch
                    {
                    }
                    try
                    {
                        logFileStream.Close();
                    }
                    catch
                    {
                    }
                    try
                    {
                        logFileStream.Dispose();
                    }
                    catch
                    {
                    }

                    logFileStream = null;
                }
                if (loggerMutex != null)
                {
                    try
                    {
                        loggerMutex.Close();
                    }
                    catch
                    {
                    }
                    loggerMutex = null;
                }

                traceLoggerHasBeenDisposed = true;
            }
        }

        /// <summary>
        /// Disposes of the TraceLogger object
        /// </summary>
        /// <remarks></remarks>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Public members

        /// <summary>
        /// Write a message to the trace log
        /// </summary>
        /// <param name="identifier">Member name or function name.</param>
        /// <param name="message">Message text</param>
        /// <remarks>
        /// </remarks>
        public void LogMessage(string identifier, string message)
        {
            bool gotMutex;

            // Return immediately if the logger is not enabled
            if (!Enabled) return;

            // Ignore attempts to write to the logger after it is disposed
            if (traceLoggerHasBeenDisposed) return;

            // Get the trace logger mutex
            try
            {
                gotMutex = loggerMutex.WaitOne(MUTEX_WAIT_TIME, false);
            }
            catch (AbandonedMutexException ex)
            {
                throw new DriverException($"TraceLogger - Abandoned Mutex Exception for file {LogFileName}, in method {identifier}, when writing message: '{message}'. See inner exception for detail", ex);
            }

            if (!gotMutex)
            {
                throw new DriverException($"TraceLogger - Timed out waiting for TraceLogger mutex after {MUTEX_WAIT_TIME}ms in method {identifier}, when writing message: '{message}'");
            }

            // We have the mutex and can now persist the message
            try
            {
                // Create the log file if it doesn't yet exist
                if (logFileStream == null) CreateLogFile();

                // Right pad the identifier string to the required column width
                identifier = identifier.PadRight(identifierWidthValue);

                // Get a DateTime object in either local or UTC time as determined by configuration
                DateTime messageDateTime = DateTimeNow();

                // Write the message to the log file
                logFileStream.WriteLine($"{messageDateTime:HH:mm:ss.fff} {MakePrintable(identifier)} {MakePrintable(message)}");
                logFileStream.Flush(); // Flush to make absolutely sure that the data is persisted to disk and can't be lost in an application crash

                // Update the day on which the last message was written
            }
            catch (Exception ex)
            {
                throw new DriverException($"TraceLogger - Exception formatting message '{identifier}' - '{message}': {ex.Message}. See inner exception for details", ex);
            }
            finally
            {
                loggerMutex.ReleaseMutex();
            }
        }

        public void LogMessageCrLf(string identifier, string message)
        {
            LogMessage(identifier, message);
        }

        /// <summary>
        /// Insert a blank line into the log file
        /// </summary>
        /// <remarks></remarks>
        public void BlankLine()
        {
            if (traceLoggerHasBeenDisposed) return;
            LogMessage("", "");
        }

        /// <summary>
        /// Enable or disable logging to the file.
        /// </summary>
        /// <value>True to enable logging</value>
        /// <returns>Boolean, current logging status (enabled/disabled).</returns>
        /// <remarks>If this property is False, calls to LogMessage do nothing. If True, messages are written to the log file.</remarks>
        public bool Enabled { get; set; }

        /// <summary>
        /// File name of the log file being created
        /// </summary>
        /// <value>Filename of the log file without the path.</value>
        /// <returns>String filename</returns>
        /// <remarks>This call will return an empty string until the first line has been written to the log file because the file is not created until required.</remarks>
        public string LogFileName { get; set; }

        /// <summary>
        /// Path to the directory in which the log file will be created
        /// </summary>
        /// <returns>String path</returns>
        /// <remarks>This call will return an empty string until the first line has been written to the log file because the file is not created until required.</remarks>
        public string LogFilePath { get; set; }

        /// <summary>
        /// Set or return the width of the identifier field in the log message
        /// </summary>
        /// <value>Width of the identifier field</value>
        /// <returns>Integer identifier width</returns>
        /// <exception cref="InvalidValueException">If the width is less than 0</exception>
        /// <remarks>Introduced with Platform 6.4.<para>If set, this width will be used instead of the default identifier field width.</para></remarks>
        public int IdentifierWidth
        {
            get
            {
                return identifierWidthValue;
            }
            set
            {
                if (value < 0) throw new InvalidValueException("IdentifierWidth", value.ToString(), "0", int.MaxValue.ToString("N0"));
                identifierWidthValue = value;
            }
        }

        /// <summary>
        /// Set True to use UTC time, set false to use Local time (default true)
        /// </summary>
        public bool UseUtcTime { get; set; }

        /// <summary>
        /// Set True to retain carriage return and line feed as control characters. Set false to translate these to [XX] format (default true)
        /// </summary>
        public bool RespectCrLf { get; set; }

        #endregion

        #region Support code

        /// <summary>
        /// Create the stream writer that will write to the log file
        /// </summary>
        private void CreateLogFile()
        {
            // Initialise working copy of the log file path
            string logFilePath;

            int logFileSuffixInteger = 0; // Initialise suffix to 0

            // Establish the path to the log file, auto generating this if required
            try
            {
                if (autoGenerateFilePath) // We need to auto generate the file path
                {
                    if (!string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.Personal))) // This is a normaL "User" account
                    {
                        logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), AUTO_PATH_BASE_DIRECTORY, string.Format(AUTO_PATH_WINDOWS_DIRECTORY_TEMPLATE, DateTimeNow()));
                    }
                    else // This is the "System" account, which does not have a personal documents directory so put log files in the 
                    {
                        logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), AUTO_PATH_WINDOWS_SYSTEM_USER_BASE_DIRECTORY, string.Format(AUTO_PATH_WINDOWS_DIRECTORY_TEMPLATE, DateTimeNow()));
                    }
                }
                else // We need to use the supplied log file path, which is already in the logFilePath property
                {
                    logFilePath = Path.Combine(LogFilePath, string.Format(AUTO_PATH_WINDOWS_DIRECTORY_TEMPLATE, DateTimeNow()));
                }
            }
            catch (Exception ex)
            {
                throw new DriverException($"TraceLogger - Unable to find determine the log file path, IsWindows: {RuntimeInformation.IsOSPlatform(OSPlatform.Windows)}. {ex.Message}. See inner exception for details", ex);
            }

            // Create the directory if required
            try
            {
                Directory.CreateDirectory(logFilePath);
            }
            catch (Exception ex)
            {
                throw new DriverException($"TraceLogger - Unable to create log file directory '{logFilePath}': {ex.Message}. See inner exception for details", ex);
            }

            // Create the log file stream writer auto a file name if required
            if (autoGenerateFileName) // We need to auto-generate a file name ourselves
            {
                try
                {
                    // Create a unique log file name based on date, time and required name by incrementing an arbitrary final suffixed count value
                    do
                    {
                        LogFileName = string.Format(AUTO_FILE_NAME_TEMPLATE, logFileType, DateTimeNow(), logFileSuffixInteger);
                        checked { ++logFileSuffixInteger; } // Increment the counter to ensure that no log file can have the same name as any other
                    }
                    while (File.Exists(Path.Combine(logFilePath, LogFileName)) & (logFileSuffixInteger <= MAXIMUM_UNIQUE_SUFFIX_ATTEMPTS)); // Loop until the generated file name does not exist or we hit the maximum number of attempts

                    // Close any current file stream before creating a new one
                    if (logFileStream is not null)
                    {
                        logFileStream.Close();
                        logFileStream.Dispose();
                    }

                    // Create the stream writer used to write to disk
                    logFileStream = new StreamWriter(Path.Combine(logFilePath, LogFileName), false);
                    logFileStream.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    throw new DriverException($"TraceLogger - Unable to create auto-generated log file '{LogFileName}' in directory '{logFilePath}': {ex.Message}. See inner exception for details", ex);
                }
            }
            else // We need to use the supplied file name
            {
                try
                {
                    // Close any current file stream before creating a new one
                    if (logFileStream is not null)
                    {
                        logFileStream.Close();
                        logFileStream.Dispose();
                    }

                    // Create the stream writer used to write to disk
                    logFileStream = new StreamWriter(Path.Combine(logFilePath, LogFileName), false);
                    logFileStream.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    throw new DriverException($"TraceLogger - Unable to create log file '{LogFileName}' in directory '{logFilePath}': {ex.Message}. See inner exception for details", ex);
                }
            }
        }

        /// <summary>
        /// Translate control characters into printable versions 
        /// </summary>
        /// <param name="message">Message to be cleansed</param>
        /// <returns>Cleaned message string</returns>
        /// <remarks>Non printable ASCII characters 0::31 and 127 are translated to [XX] format where XX is a two digit hex code. 
        /// Characters 13 and 10 (carriage return and linefeed) are either translated or left as control characters according to the setting of the RespectCrLf property.
        /// The default is for these to be left as control characters so that exception stack dumps print properly.</remarks>
        private string MakePrintable(string message)
        {
            string printableMessage = "";
            int charNo;

            // Present any unprintable characters in [0xHH] format
            foreach (char c in message)
            {
                charNo = (int)c;
                switch (charNo)
                {
                    case int i when i == 10 && RespectCrLf: // Handle carriage return and line feed as "printable" characters if "Respect CrLf" is enabled
                    case int j when j == 13 && RespectCrLf:
                        {
                            printableMessage += c.ToString();
                            break;
                        }

                    case int i when (i >= 0 && i <= 31) || i == 127: // Represent "non-printable" characters as hex codes
                        {
                            printableMessage += "[" + charNo.ToString("X2") + "]";
                            break;
                        }

                    default: // Handle everything else as "printable" characters 
                        {
                            printableMessage += c.ToString();
                            break;
                        }
                }
            }
            return printableMessage; // Return the formatted printable message
        }

        /// <summary>
        /// Return a dateTime object reflecting Local or UTC time depending on the setting of the UseUtcTime property.
        /// </summary>
        /// <returns>DateTime object in either local or UTC time.</returns>
        private DateTime DateTimeNow()
        {
            if (UseUtcTime)
            {
                return DateTime.UtcNow;
            }
            else
            {
                return DateTime.Now;
            }
        }

        #endregion

        public bool DebugTraceState { get; set; }

        public bool IpAddressTraceState { get; set; }

        public LogLevel LoggingLevel => throw new System.NotImplementedException();

        public void LogMessage(uint instance, string prefix, string message)
        {
            LogMessage(prefix + " " + instance.ToString(), message);
        }

        public void LogMessage(uint clientID, uint clientTransactionID, uint serverTransactionID, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, "", prefix, message));
            }
            else
            {
                LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessage(RequestData requestData, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, requestData.ClientIpAddress, prefix, message));
            }
            else
            {
                LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessageCrLf(uint instance, string prefix, string message)
        {
            LogMessage(prefix + " " + instance.ToString(), message);
        }

        public void LogMessageCrLf(uint clientID, uint clientTransactionID, uint serverTransactionID, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, "", prefix, message));
            }
            else
            {
                LogMessage(string.Format(ID_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void LogMessageCrLf(RequestData requestData, string prefix, string message)
        {
            if (IpAddressTraceState)
            {
                LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_WITH_IP_ADDRESS, requestData.ClientIpAddress, prefix, message));
            }
            else
            {
                LogMessage(string.Format(ID_FORMAT_STRING, requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID), string.Format(MESSAGE_FORMAT_STRING_NO_IP_ADDRESS, prefix, message));
            }
        }

        public void SetMinimumLoggingLevel(LogLevel level)
        {
            throw new System.NotImplementedException();
        }

        public void Log(LogLevel level, string message)
        {
            throw new System.NotImplementedException();
        }

        #region Used ITracelogger members

        #endregion

        #region Unused ITraceLogger members

        //void ITraceLogger.LogStart(string Identifier, string Message)
        //{
        //    throw new MethodNotImplementedException("LogStart(string Identifier, string Message)");
        //}

        //void ITraceLogger.LogContinue(string Message, bool HexDump)
        //{
        //    throw new MethodNotImplementedException("LogContinue(string Message, bool HexDump)");
        //}

        //void ITraceLogger.LogFinish(string Message, bool HexDump)
        //{
        //    throw new MethodNotImplementedException("LogFinish(string Message, bool HexDump)");
        //}

        //void ITraceLogger.LogIssue(string Identifier, string Message)
        //{
        //    throw new MethodNotImplementedException("e(string Identifier, string Message)");
        //}

        //void ITraceLogger.SetLogFile(string LogFileName, string LogFileType)
        //{
        //    throw new MethodNotImplementedException("SetLogFile(string LogFileName, string LogFileType)");
        //}

        #endregion
    }
}