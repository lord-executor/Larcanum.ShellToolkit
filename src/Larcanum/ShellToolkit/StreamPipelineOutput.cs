namespace Larcanum.ShellToolkit;

public class StreamPipelineOutput : IPipelineOutput
{
    public Stream Out { get; }
    public Stream Error { get; } = Stream.Null;

    public StreamPipelineOutput(Stream output)
    {
        Out = output;
    }

    public Task<int> WaitForExit(CancellationToken ct = default)
    {
        return Task.FromResult(0);
    }
}
