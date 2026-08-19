namespace Larcanum.ShellToolkit;

public enum CommandMode
{
    Run,
    Capture,
}

public static class CommandModeExtensions
{
    extension(CommandMode mode)
    {
        public string AsString() => mode switch
        {
            CommandMode.Run => "(r)",
            CommandMode.Capture => "(c)",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
