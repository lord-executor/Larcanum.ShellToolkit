namespace Larcanum.ShellToolkit;

public interface IPipelineStep
{
    Task<IPipelineOutput> Connect(IPipelineOutput? previous, OutputMode mode, CancellationToken ct = default);
}
