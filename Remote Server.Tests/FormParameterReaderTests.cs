using ASCOM.Remote;
using System.Collections.Specialized;
using System.Text;
using Xunit;

namespace Remote_Server.Tests;

public sealed class FormParameterReaderTests
{
    [Fact]
    public void TryRead_ParsesRepresentativeAlpacaDeviceCommands()
    {
        const string body =
            "ClientID=101&ClientTransactionID=202&RightAscension=12.345" +
            "&Declination=-45.67&Position=12345&Duration=0.25&Light=true";

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Equal("101", parameters["ClientID"]);
        Assert.Equal("202", parameters["ClientTransactionID"]);
        Assert.Equal("12.345", parameters["RightAscension"]);
        Assert.Equal("-45.67", parameters["Declination"]);
        Assert.Equal("12345", parameters["Position"]);
        Assert.Equal("0.25", parameters["Duration"]);
        Assert.Equal("true", parameters["Light"]);
    }

    [Fact]
    public void TryRead_PreservesUnescapedEqualsCharactersInGenericActionParameters()
    {
        const string body =
            "ClientID=7&ClientTransactionID=8&Action=Configure" +
            "&Parameters=mode=fast=calibrated";

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Equal("mode=fast=calibrated", parameters["Parameters"]);
        Assert.Equal("mode", ParseUsingLegacySplit(body)["Parameters"]);
    }

    [Fact]
    public void TryRead_PreservesCaseSensitiveParameterNames()
    {
        const string body = "ClientID=1&clientid=2";

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Equal("1", parameters["ClientID"]);
        Assert.Equal("2", parameters["clientid"]);
    }

    [Fact]
    public void TryRead_StripsUtf8ByteOrderMarkLikeThePreviousStreamReader()
    {
        byte[] body = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes("ClientID=1&ClientTransactionID=2"))
            .ToArray();
        using MemoryStream input = new(body, writable: false);

        bool success = FormParameterReader.TryRead(input, Encoding.UTF8,
            body.Length, out _, out NameValueCollection parameters, out _,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Equal("1", parameters["ClientID"]);
        Assert.Equal("2", parameters["ClientTransactionID"]);
    }

    [Fact]
    public void TryRead_RejectsMoreThanMaximumFormParameterSegments()
    {
        string body = string.Join("&", Enumerable.Range(0,
            ServerForm.MAXIMUM_FORM_PARAMETER_COUNT + 1)
            .Select(index => $"Parameter{index}=value"));

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.False(success);
        Assert.Empty(parameters);
        Assert.Contains("parameters", errorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryRead_AcceptsExactlyMaximumFormParameterSegments()
    {
        string body = string.Join("&", Enumerable.Range(0,
            ServerForm.MAXIMUM_FORM_PARAMETER_COUNT)
            .Select(index => $"Parameter{index}=value"));

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Equal(ServerForm.MAXIMUM_FORM_PARAMETER_COUNT,
            parameters.Count);
    }

    [Fact]
    public void TryRead_RejectsMoreThanMaximumEmptyFormParameterSegments()
    {
        string body = new('&', ServerForm.MAXIMUM_FORM_PARAMETER_COUNT);

        bool success = TryRead(body, out NameValueCollection parameters,
            out string errorMessage);

        Assert.False(success);
        Assert.Empty(parameters);
        Assert.Contains("parameters", errorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryRead_RejectsOversizedDeclaredBodyWithoutReadingInput()
    {
        using ThrowOnReadStream input = new();

        bool success = FormParameterReader.TryRead(
            input,
            Encoding.UTF8,
            ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES + 1L,
            out _,
            out NameValueCollection parameters,
            out _,
            out string errorMessage);

        Assert.False(success);
        Assert.Empty(parameters);
        Assert.Contains("maximum", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, input.ReadCount);
    }

    [Fact]
    public void TryRead_RejectsOversizedChunkedBodyAfterBoundedRead()
    {
        byte[] body = Encoding.UTF8.GetBytes(
            "Parameters=" + new string('x',
                ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES));
        using CountingReadStream input = new(body);

        bool success = FormParameterReader.TryRead(
            input,
            Encoding.UTF8,
            -1,
            out _,
            out NameValueCollection parameters,
            out _,
            out string errorMessage);

        Assert.False(success);
        Assert.Empty(parameters);
        Assert.Contains("maximum", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ServerForm.MAXIMUM_FORM_BODY_SIZE_BYTES + 1L,
            input.BytesRead);
    }

    [Fact]
    public void TryRead_AcceptsAnEmptyBody()
    {
        bool success = TryRead(string.Empty, out NameValueCollection parameters,
            out string errorMessage);

        Assert.True(success, errorMessage);
        Assert.Empty(parameters);
    }

    private static bool TryRead(string body, out NameValueCollection parameters,
        out string errorMessage)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        using MemoryStream input = new(bytes, writable: false);
        return FormParameterReader.TryRead(input, Encoding.UTF8, bytes.Length,
            out _, out parameters, out _, out errorMessage);
    }

    private static NameValueCollection ParseUsingLegacySplit(string body)
    {
        NameValueCollection parameters = new(StringComparer.Ordinal);
        foreach (string parameter in body.Split('&'))
        {
            string[] keyValuePair = parameter.Split('=');
            string key = keyValuePair[0].Trim();
            if (!string.IsNullOrEmpty(key))
            {
                string value = keyValuePair.Length > 1
                    ? System.Web.HttpUtility.UrlDecode(keyValuePair[1].Trim())
                    : string.Empty;
                parameters.Add(key, value);
            }
        }

        return parameters;
    }

    private sealed class ThrowOnReadStream : Stream
    {
        public int ReadCount { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCount++;
            throw new InvalidOperationException("The input stream must not be read.");
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
    }

    private sealed class CountingReadStream : MemoryStream
    {
        public CountingReadStream(byte[] buffer) : base(buffer, writable: false) { }

        public long BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = base.Read(buffer, offset, count);
            BytesRead += bytesRead;
            return bytesRead;
        }
    }
}
