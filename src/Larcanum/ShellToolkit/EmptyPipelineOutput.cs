namespace Larcanum.ShellToolkit;

public class EmptyPipelineOutput : IPipelineOutput
{
    public static readonly EmptyPipelineOutput Instance = new EmptyPipelineOutput();

    private readonly Task<int> _task;

    public Stream Out { get; } = Stream.Null;
    public Stream Error { get; } = Stream.Null;

    private EmptyPipelineOutput() : this(Task.FromResult(0)) {}

    public EmptyPipelineOutput(Task<int> task)
    {
        _task = task;
    }

    public Task<int> WaitForExit(CancellationToken ct = default)
    {
        return _task;
    }
}
