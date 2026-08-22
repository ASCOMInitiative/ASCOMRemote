using ASCOM.Remote;
using System.Text.RegularExpressions;
using Xunit;

namespace Remote_Server.Tests;

public sealed class TraceLoggerPlusTests : IDisposable
{
    private readonly string testRoot = Path.Combine(
        Path.GetTempPath(),
        $"ASCOMRemoteTests-{Guid.NewGuid():N}");

    [Fact]
    public void LogMessage_RollsAutomaticallyNamedFileAtConfiguredSize()
    {
        TraceLoggerPlus logger = CreateLogger("SizeBounded");
        try
        {
            logger.MaximumLogFileSizeBytes = 1;
            logger.MaximumRetainedLogFiles = 10;

            logger.LogMessage("Test", "First message");
            logger.LogMessage("Test", "Second message");

            string[] logFiles = FindLogs("SizeBounded");

            Assert.Equal(2, logFiles.Length);
        }
        finally
        {
            logger.Dispose();
        }
    }

    [Fact]
    public void LogMessage_RetainsOnlyConfiguredFilesForSameLoggerType()
    {
        string yesterdayFolder = CreateDailyFolder(DateTime.Now.AddDays(-1));
        string todayFolder = CreateDailyFolder(DateTime.Now);
        string[] boundedLogs =
        [
            CreateOldLog(yesterdayFolder, "RetentionBounded", 0),
            CreateOldLog(yesterdayFolder, "RetentionBounded", 1),
            CreateOldLog(todayFolder, "RetentionBounded", 2),
            CreateOldLog(todayFolder, "RetentionBounded", 3)
        ];
        string otherLog = CreateOldLog(yesterdayFolder, "OtherLogger", 0);
        string unrelatedFile = Path.Combine(todayFolder, "notes.txt");
        string lookalikeFile = Path.Combine(
            todayFolder,
            "ASCOM.RetentionBounded.manual-backup.txt");
        File.WriteAllText(unrelatedFile, "preserve me");
        File.WriteAllText(lookalikeFile, "preserve me too");

        TraceLoggerPlus logger = CreateLogger("RetentionBounded");
        try
        {
            logger.MaximumRetainedLogFiles = 3;
            logger.LogMessage("Test", "Current message");

            Assert.Equal(3, FindAutomaticallyNamedLogs("RetentionBounded").Length);
            Assert.True(File.Exists(otherLog));
            Assert.True(File.Exists(unrelatedFile));
            Assert.True(File.Exists(lookalikeFile));
            Assert.False(File.Exists(boundedLogs[0]));
            Assert.False(File.Exists(boundedLogs[1]));
        }
        finally
        {
            logger.Dispose();
        }
    }

    [Fact]
    public void LogMessage_RetriesRolloverAfterFileCreationFailure()
    {
        FailOnceTraceLoggerPlus logger = new(testRoot, "RecoverableRollover")
        {
            MaximumLogFileSizeBytes = 1,
            MaximumRetainedLogFiles = 10
        };

        try
        {
            logger.LogMessage("Test", "First message");
            logger.FailNextCreate = true;

            Assert.Throws<ASCOM.DriverException>(() =>
                logger.LogMessage("Test", "Message that triggers failure"));

            logger.LogMessage("Test", "Message after recovery");

            Assert.Equal(2, FindAutomaticallyNamedLogs("RecoverableRollover").Length);
        }
        finally
        {
            logger.Dispose();
        }
    }

    [Fact]
    public void LogMessage_DoesNotLimitGenericCallersByDefault()
    {
        string dailyFolder = CreateDailyFolder(DateTime.Now);
        string oldLog = CreateOldLog(dailyFolder, "UnlimitedByDefault", 0);
        TraceLoggerPlus logger = CreateLogger("UnlimitedByDefault");

        try
        {
            logger.LogMessage("Test", "First message");
            logger.LogMessage("Test", "Second message");

            Assert.Equal(2, FindAutomaticallyNamedLogs("UnlimitedByDefault").Length);
            Assert.True(File.Exists(oldLog));
        }
        finally
        {
            logger.Dispose();
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, true);
        }
    }

    private TraceLoggerPlus CreateLogger(string loggerType)
    {
        Directory.CreateDirectory(testRoot);
        return new TraceLoggerPlus("", testRoot, loggerType, true);
    }

    private string CreateDailyFolder(DateTime date)
    {
        string dailyFolder = Path.Combine(
            testRoot,
            $"Logs {date:yyyy-MM-dd}");
        Directory.CreateDirectory(dailyFolder);
        return dailyFolder;
    }

    private static string CreateOldLog(
        string dailyFolder,
        string loggerType,
        int index)
    {
        string path = Path.Combine(
            dailyFolder,
            $"ASCOM.{loggerType}.0000.00000{index}.txt");
        File.WriteAllText(path, $"old log {index}");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(index - 10));
        return path;
    }

    private string[] FindLogs(string loggerType)
    {
        return Directory.GetFiles(
            testRoot,
            $"ASCOM.{loggerType}.*.txt",
            SearchOption.AllDirectories);
    }

    private string[] FindAutomaticallyNamedLogs(string loggerType)
    {
        Regex automaticFileNamePattern = new(
            $"^ASCOM\\.{Regex.Escape(loggerType)}\\.\\d{{4}}\\.\\d{{6,7}}\\.txt$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        return FindLogs(loggerType)
            .Where(path => automaticFileNamePattern.IsMatch(Path.GetFileName(path)))
            .ToArray();
    }

    private sealed class FailOnceTraceLoggerPlus(
        string logRoot,
        string loggerType) : TraceLoggerPlus("", logRoot, loggerType, true)
    {
        public bool FailNextCreate { get; set; }

        protected override StreamWriter CreateStreamWriter(string filePath)
        {
            if (FailNextCreate)
            {
                FailNextCreate = false;
                throw new IOException("Simulated log file creation failure.");
            }

            return base.CreateStreamWriter(filePath);
        }
    }
}
