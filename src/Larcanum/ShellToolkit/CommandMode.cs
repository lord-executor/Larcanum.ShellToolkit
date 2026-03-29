namespace Larcanum.ShellToolkit;

public enum CommandMode
{
    Run,
    Capture,
    Detach
}

public static class CommandModeExtensions
{
    extension(CommandMode mode)
    {
        public string AsString() => mode switch
        {
            CommandMode.Run => "(r)",
            CommandMode.Capture => "(c)",
            CommandMode.Detach => "(d)",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
