namespace Larcanum.ShellToolkit;

public class EmptyPipelineOutput : IPipelineOutput
{
    public static readonly EmptyPipelineOutput Instance = new EmptyPipelineOutput();

    public StreamReader? Out => null;

    private EmptyPipelineOutput() {}

    public Task<int> WaitForExit(CancellationToken ct = default)
    {
        return Task.FromResult(0);
    }
}
