namespace Larcanum.ShellToolkit;

public class StreamPipelineOutput : IPipelineOutput
{
    private readonly Task<int> _task;

    public Stream Out { get; }
    public Stream Error { get; }

    public StreamPipelineOutput(Stream output)
        : this(output, Stream.Null, Task.FromResult(0))
    {
        Out = output;
    }

    public StreamPipelineOutput(Stream output, Stream error)
        : this(output, error, Task.FromResult(0))
    {
        Out = output;
        Error = error;
    }

    public StreamPipelineOutput(Stream output, Stream error, Task<int> task)
    {
        _task = task;
        Out = output;
        Error = error;
    }

    public Task<int> WaitForExit(CancellationToken ct = default)
    {
        return _task;
    }
}
