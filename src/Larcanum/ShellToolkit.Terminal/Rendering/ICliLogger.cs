using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit.Terminal.Rendering;

public interface ICliLogger : ILogger
{
    void SetLogLevel(LogLevel level);
    void WriteOutput(AnsiTextSpan span);
    void WriteError(AnsiTextSpan span);
    void WriteHost(AnsiTextSpan span);
    string? Prompt(string promptLine);

    bool PromptYesNo(string promptLine)
    {
        return Prompt($"{promptLine} [y(es)/n(o)]:")?.StartsWith('y') ?? false;
    }
}
