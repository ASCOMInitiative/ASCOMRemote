using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.Remote
{
    public abstract class RestResponseBase
    {
        private Exception exception;
        private int errorNumber = 0;
        private string errorMessage = "";

        public int ClientTransactionID { get; set; }
        public int ServerTransactionID { get; set; }
        public string Method { get; set; }
        public int ErrorNumber
        {
            get
            {
                return errorNumber;
            }
        }
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
        }
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
                    errorNumber = exception.HResult;
                    errorMessage = exception.Message;
                }
            }
        }
    }
}
