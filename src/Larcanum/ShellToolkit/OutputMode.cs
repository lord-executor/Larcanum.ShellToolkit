namespace Larcanum.ShellToolkit;

public enum OutputMode
{
    /// <summary>
    /// Output is piped into the next command if there is one or written to the terminal if there is no next command.
    /// </summary>
    Default,
    /// <summary>
    /// Output is captured in <see cref="CommandResult.Output"/>
    /// </summary>
    Capture,
    /// <summary>
    /// Output is discarded.
    /// </summary>
    None,
}
