
using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit.Terminal.Rendering;

public class CliLogger : ICliLogger, IDisposable
{
    private static readonly IDictionary<LogLevel, Func<string, AnsiTextSpan>> _spanMapping =
        new Dictionary<LogLevel, Func<string, AnsiTextSpan>>
        {
            [LogLevel.Trace] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.LightGray),
            [LogLevel.Debug] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.LightCyan),
            [LogLevel.Information] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.LightBlue),
            [LogLevel.Warning] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.Yellow),
            [LogLevel.Error] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.Red),
            [LogLevel.Critical] = msg => new AnsiTextSpan(msg, true).WithForegroundColor(Ansi.Color.Foreground.Red),
        };

    private readonly StandardStreams _standardStreams;
    private LogLevel _threshold;
    private readonly TextWriter? _hostWriter;

    public CliLogger(StandardStreams standardStreams, LogLevel threshold, bool enableHostWriter = false)
    {
        _standardStreams = standardStreams;
        _threshold = threshold;

        if (enableHostWriter)
        {
            // Unfortunately there does not seem to be a .NET native way or even an otherwise _reasonable_ way to
            // write directly to the console host without going through STDOUT or STDERR. The only thing that I found
            // which at least works is to use this very archaic and wildly undocumented "\\.\CON" DOS-era pseudo-file.
            // See https://learn.microsoft.com/en-us/dotnet/standard/io/file-path-formats#handle-legacy-devices
            var ttyPath = (Environment.OSVersion.Platform == PlatformID.Win32NT
                           || Environment.OSVersion.Platform == PlatformID.Win32S
                           || Environment.OSVersion.Platform == PlatformID.Win32Windows
                           || Environment.OSVersion.Platform == PlatformID.WinCE)
                ? @"\\.\CON"
                : "/dev/tty";

            if (File.Exists(ttyPath))
            {
                var writer = new StreamWriter(File.OpenWrite(ttyPath));
                writer.AutoFlush = true;

                _hostWriter = writer;
            }
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var span = _spanMapping[logLevel](formatter(state, exception));

            if (logLevel >= LogLevel.Error)
            {
                WriteError(span);
            }
            else
            {
                WriteOutput(span);
            }
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _threshold;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public void SetLogLevel(LogLevel level)
    {
        _threshold = level;
    }

    public void WriteOutput(AnsiTextSpan span)
    {
        span.WriteTo(_standardStreams.Output, AnsiOutputMode.Ansi);
    }

    public void WriteError(AnsiTextSpan span)
    {
        span.WriteTo(_standardStreams.Error, AnsiOutputMode.Ansi);
    }

    public void WriteHost(AnsiTextSpan span)
    {
        if (_hostWriter != null)
        {
            span.WriteTo(_hostWriter, AnsiOutputMode.Ansi);
        }
        else
        {
            // If the host writer is not available (for whatever reason), we fall back to writing that to
            // STDERR. While not ideal, it at least prevents the issue of "host" messages polluting the STDOUT stream.
            span.WriteTo(_standardStreams.Error, AnsiOutputMode.Ansi);
        }
    }

    public string? Prompt(string promptLine)
    {
        WriteHost(new AnsiTextSpan($"{promptLine} "));
        return _standardStreams.Input.ReadLine();
    }

    public void Dispose()
    {
        _hostWriter?.Dispose();
    }
}
