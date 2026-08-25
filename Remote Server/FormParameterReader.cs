using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace ASCOM.Remote
{
    /// <summary>
    /// Reads bounded application/x-www-form-urlencoded request bodies while
    /// preserving the ASCOM Remote form-parameter collection semantics.
    /// </summary>
    public static class FormParameterReader
    {
        /// <summary>
        /// Maximum accepted form-body size in bytes.
        /// </summary>
        public const int MAXIMUM_FORM_BODY_SIZE_BYTES = 1024 * 1024;

        /// <summary>
        /// Maximum accepted form parameters, including empty parameter segments.
        /// </summary>
        public const int MAXIMUM_FORM_PARAMETER_COUNT = 256;

        /// <summary>
        /// Reads and parses one form body.
        /// </summary>
        public static bool TryRead(
            Stream inputStream,
            Encoding contentEncoding,
            long contentLength,
            out string formParameterString,
            out NameValueCollection formParameters,
            out int formParameterCount,
            out string errorMessage)
        {
            if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));
            if (contentEncoding is null) throw new ArgumentNullException(nameof(contentEncoding));

            formParameterString = string.Empty;
            formParameters = new NameValueCollection(StringComparer.Ordinal);
            formParameterCount = 0;
            errorMessage = string.Empty;

            if (contentLength > MAXIMUM_FORM_BODY_SIZE_BYTES)
            {
                errorMessage = $"The form request body exceeds the maximum permitted size of {MAXIMUM_FORM_BODY_SIZE_BYTES} bytes.";
                return false;
            }

            if (contentLength < -1)
            {
                errorMessage = "The form request body has an invalid Content-Length.";
                return false;
            }

            int initialCapacity = contentLength > 0 ? (int)contentLength : 0;
            using MemoryStream formBody = new(initialCapacity);
            byte[] buffer = new byte[8192];
            long totalBytesRead = 0;

            while (true)
            {
                int remainingBytesToCheck = (int)Math.Min(
                    buffer.Length,
                    MAXIMUM_FORM_BODY_SIZE_BYTES + 1L - totalBytesRead);
                int bytesRead = inputStream.Read(buffer, 0, remainingBytesToCheck);

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;
                if (totalBytesRead > MAXIMUM_FORM_BODY_SIZE_BYTES)
                {
                    errorMessage = $"The form request body exceeds the maximum permitted size of {MAXIMUM_FORM_BODY_SIZE_BYTES} bytes.";
                    return false;
                }

                formBody.Write(buffer, 0, bytesRead);
            }

            formBody.Position = 0;
            using (StreamReader reader = new(formBody, contentEncoding, true, 1024, true))
            {
                formParameterString = reader.ReadToEnd();
            }

            if (formParameterString.Length == 0)
            {
                return true;
            }

            int parameterStartIndex = 0;
            while (parameterStartIndex <= formParameterString.Length)
            {
                int parameterEndIndex = formParameterString.IndexOf('&', parameterStartIndex);
                int parameterLength = parameterEndIndex < 0
                    ? formParameterString.Length - parameterStartIndex
                    : parameterEndIndex - parameterStartIndex;
                formParameterCount++;
                if (formParameterCount > MAXIMUM_FORM_PARAMETER_COUNT)
                {
                    formParameters.Clear();
                    errorMessage = $"The form request body exceeds the maximum permitted number of {MAXIMUM_FORM_PARAMETER_COUNT} parameters.";
                    return false;
                }

                string parameter = formParameterString.Substring(parameterStartIndex, parameterLength);
                int separatorIndex = parameter.IndexOf('=');
                string key = (separatorIndex < 0 ? parameter : parameter[..separatorIndex]).Trim();
                string value = separatorIndex < 0
                    ? string.Empty
                    : HttpUtility.UrlDecode(parameter[(separatorIndex + 1)..].Trim());
                if (!string.IsNullOrEmpty(key))
                {
                    formParameters.Add(key, value);
                }

                if (parameterEndIndex < 0)
                {
                    break;
                }

                parameterStartIndex = parameterEndIndex + 1;
            }

            return true;
        }
    }
}
