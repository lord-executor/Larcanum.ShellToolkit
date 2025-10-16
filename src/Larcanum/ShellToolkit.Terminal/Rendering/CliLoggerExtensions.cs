namespace Larcanum.ShellToolkit.Terminal.Rendering;

public static class CliLoggerExtensions
{
    public static void LogContent(this ICliLogger logger, string message)
    {
        logger.WriteOutput(new AnsiTextSpan(message, true));
    }
}
