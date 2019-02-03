using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.Remote
{
    public abstract class RestResponseBase
    {
        private Exception exception;

        public int ClientTransactionID { get; set; }
        public int ServerTransactionID { get; set; }
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
    }
}
