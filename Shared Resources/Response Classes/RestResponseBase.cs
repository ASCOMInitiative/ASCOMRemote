using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.Remote
{
    public abstract class RestResponseBase
    {
        private Exception exception;

        public uint ClientTransactionID { get; set; }
        public uint ServerTransactionID { get; set; }
        public int ErrorNumber { get; set; } = 0;
        public string ErrorMessage { get; set; } = "";
        public Exception DriverException
        {
            get
            {
                return exception;
            }
            set
            {
                exception = value;
                if (exception != null)
                {
                    ErrorNumber = exception.HResult;
                    ErrorMessage = exception.Message;
                }
            }
        }

        /// <summary>
        /// Method used by NewtonSoft JSON to determine whether the DriverException field should be included in the serialise JSON response.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeDriverException()
        {
            return SerializeDriverException;
        }

        /// <summary>
        /// Control variable that determines whether the DriverException field will be included in serialised JSON responses
        /// </summary>
        internal bool SerializeDriverException { get; set; } = true;
    }
}
