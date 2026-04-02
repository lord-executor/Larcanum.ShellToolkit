namespace Larcanum.ShellToolkit;

public class StreamPipelineOutput : IPipelineOutput
{
    public StreamReader? Out { get; }

    public StreamPipelineOutput(StreamReader input)
    {
        Out = input;
    }

    public StreamPipelineOutput(Stream input)
        : this(new StreamReader(input))
    {
    }

    public Task<int> WaitForExit(CancellationToken ct = default)
    {
        return Task.FromResult(0);
    }
}
