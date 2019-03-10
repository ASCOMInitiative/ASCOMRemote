using System;
using System.Runtime.Serialization;

namespace ASCOM.Remote
{
    /// <summary>
    ///   This exception should be raised by the driver to reject a parameter from the client. 
    /// </summary>
    [Serializable]
    public class InvalidParameterException : DriverException
    {
        const string csDefaultMessage = "The requested operation is not permitted at this time";
        public static readonly int InvalidParameterErrorNumber = unchecked((int)0x80040FFE);

        /// <summary>
        ///   Default public constructor for NotConnectedException takes no parameters.
        /// </summary>
        public InvalidParameterException() : base(csDefaultMessage, InvalidParameterErrorNumber)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "InvalidParameterException" /> class
        ///   from another exception.
        /// </summary>
        /// <param name = "innerException">The inner exception.</param>
        public InvalidParameterException(Exception innerException) : base(csDefaultMessage, InvalidParameterErrorNumber, innerException)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "InvalidParameterException" /> class
        ///   with a non-default error message.
        /// </summary>
        /// <param name = "message">A descriptive human-readable message.</param>
        public InvalidParameterException(string message) : base(message, InvalidParameterErrorNumber)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "InvalidParameterException" /> class
        ///   based on another exception.
        /// </summary>
        /// <param name = "message">Descriptive text documenting the cause or source of the error.</param>
        /// <param name = "innerException">The inner exception the led to the throwing of this exception.</param>
        public InvalidParameterException(string message, Exception innerException) : base(message, InvalidParameterErrorNumber, innerException)
        {
        }

        /// <summary>
        ///   Added to keep Code Analysis happy
        /// </summary>
        /// <param name = "info"></param>
        /// <param name = "context"></param>
        protected InvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
