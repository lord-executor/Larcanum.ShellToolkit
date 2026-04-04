namespace Larcanum.ShellToolkit;

public class BoundPipeline : IBoundCommand
{
    private readonly IExecutionContext _context;
    private readonly IPipeline _pipeline;

    internal BoundPipeline(IExecutionContext context, IPipeline pipeline)
    {
        _context = context;
        _pipeline = pipeline;
    }

    public async Task<CommandResult> CaptureAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_pipeline, CommandMode.Capture);
        return await _pipeline.Run(_context, OutputMode.Capture, ct);
    }

    public async Task<int> ExecAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_pipeline, CommandMode.Run);
        return (await _pipeline.Run(_context, OutputMode.Default, ct)).ExitCode;
    }

    public IBoundCommand ThrowOnError()
    {
        return new BoundErrorHandler(_context, this);
    }

    public override string ToString()
    {
        return _pipeline.ToString()!;
    }
}
