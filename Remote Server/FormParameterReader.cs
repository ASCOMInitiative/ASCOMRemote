using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace ASCOM.Remote
{
    /// <summary>
    /// Reads bounded application/x-www-form-urlencoded request bodies while preserving the ASCOM Remote form-parameter collection semantics.
    /// </summary>
    /// <remarks>
    /// The constants MAXIMUM_FORM_BODY_SIZE_BYTES and MAXIMUM_FORM_PARAMETER_COUNT are defined in the ServerForm class.
    /// </remarks>
    public static class FormParameterReader
    {
        /// <summary>
        /// Reads and parses one form body.
        /// </summary>
        public static bool TryRead(Stream inputStream, Encoding contentEncoding, long contentLength, out string formParameterString, out NameValueCollection formParameters, out int formParameterCount, out string errorMessage)
        {
            // Validate required inputs.
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ArgumentNullException.ThrowIfNull(contentEncoding, nameof(contentEncoding));

            // Initialize outputs before any early return.
            formParameterString = string.Empty;
            formParameters = new NameValueCollection(StringComparer.Ordinal); // Case sensitive collection for ASCOM Remote form parameters
            formParameterCount = 0;
            errorMessage = string.Empty;

            if (contentLength > ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES)
            {
                errorMessage = $"The form request body exceeds the maximum permitted size of {ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES} bytes.";
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

            // Enforce the MAXIMUM_FORM_BODY_SIZE_BYTES limit while reading when Content-Length is absent or inaccurate. Exit if the limit is exceeded.
            while (true)
            {
                // Calculate the remaining bytes to read, ensuring we don't exceed the maximum allowed size.
                int remainingBytesToCheck = (int)Math.Min(buffer.Length, ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES + 1L - totalBytesRead);

                // Read from the input stream into the buffer, up to the remaining bytes to check or the size of the buffer.
                int bytesRead = inputStream.Read(buffer, 0, remainingBytesToCheck);

                // If we have read all bytes, break the loop.
                if (bytesRead == 0)
                    break;

                // Update the total bytes read and check if it exceeds the maximum allowed size. Exit with error if it does.
                totalBytesRead += bytesRead;
                if (totalBytesRead > ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES)
                {
                    errorMessage = $"The form request body exceeds the maximum permitted size of {ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES} bytes.";
                    return false;
                }

                // Write the read bytes into the formBody MemoryStream for later parsing.
                formBody.Write(buffer, 0, bytesRead);
            }

            // Reset the MemoryStream to the beginning before reading its content as a string.
            formBody.Position = 0;
            using (StreamReader reader = new(formBody, contentEncoding, true, 1024, true))
            {
                formParameterString = reader.ReadToEnd();
            }

            // If the form parameter string is empty, return true as there are no parameters to parse.
            if (formParameterString.Length == 0)
                return true;

            // Parse the form parameter string into key-value pairs, counting parameters and enforcing the maximum parameter count limit.
            int parameterStartIndex = 0;

            // Split manually so empty segments count toward the request limit.
            while (parameterStartIndex <= formParameterString.Length)
            {
                // Find the next '&' character to determine the end of the current parameter.
                int parameterEndIndex = formParameterString.IndexOf('&', parameterStartIndex);

                // Calculate the length of the current parameter, handling the case where there is no '&' found (i.e., it's the last parameter).
                int parameterLength = parameterEndIndex < 0 ? formParameterString.Length - parameterStartIndex : parameterEndIndex - parameterStartIndex;

                // Increment the form parameter count for each parameter found.
                formParameterCount++;

                // Enforce the MAXIMUM_FORM_PARAMETER_COUNT limit while parsing. Exit if the limit is exceeded.
                if (formParameterCount > ServerForm.MAXIMUM_FORM_PARAMETER_COUNT)
                {
                    formParameters.Clear();
                    errorMessage = $"The form request body exceeds the maximum permitted number of {ServerForm.MAXIMUM_FORM_PARAMETER_COUNT} parameters.";
                    return false;
                }

                // Extract the current parameter sub-string and split it into key and value based on the '=' character.
                string parameter = formParameterString.Substring(parameterStartIndex, parameterLength);
                int separatorIndex = parameter.IndexOf('=');
                string key = (separatorIndex < 0 ? parameter : parameter[..separatorIndex]).Trim();

                // Values are URL-decoded to preserve existing key semantics.
                string value = separatorIndex < 0 ? string.Empty : HttpUtility.UrlDecode(parameter[(separatorIndex + 1)..].Trim());

                // Add the key-value pair to the formParameters collection only if the key is not null or empty.
                if (!string.IsNullOrEmpty(key))
                    formParameters.Add(key, value);

                // Move to the next parameter by updating the start index. If there are no more parameters, break the loop.
                if (parameterEndIndex < 0)
                    break;

                // Update the start index for the next parameter, skipping the '&' character.
                parameterStartIndex = parameterEndIndex + 1;
            }

            // Successfully read and parsed the form parameters, return true.
            return true;
        }
    }
}
