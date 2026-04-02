namespace Larcanum.ShellToolkit;

public interface IPipelineOutput
{
    StreamReader? Out { get; }
    Task<int> WaitForExit(CancellationToken ct = default);
}
