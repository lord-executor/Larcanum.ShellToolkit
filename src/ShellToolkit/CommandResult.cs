namespace Larcanum.ShellToolkit;

public class CommandResult
{
    public int ExitCode { get; init; }
    public string? Output { get; init; }
    public string? Error { get; init; }

    public bool IsSuccess => ExitCode == 0;

    public string? AsNullableString()
    {
        return Output?.Trim();
    }

    public string AsString()
    {
        return AsNullableString() ?? string.Empty;
    }
}
