using Microsoft.Extensions.Logging;

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
        _context.Logger.LogDebug("[exec-bg]: {pipeline}", _pipeline);
        return await _pipeline.Run(OutputMode.Capture, ct);
    }

    public async Task<int> ExecAsync(CancellationToken ct = default)
    {
        _context.Logger.LogInformation("[exec]: {pipeline}", _pipeline);
        return (await _pipeline.Run(OutputMode.Default, ct)).ExitCode;
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
