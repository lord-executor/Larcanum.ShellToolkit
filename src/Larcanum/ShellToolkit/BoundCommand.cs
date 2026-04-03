namespace Larcanum.ShellToolkit;

public class BoundCommand : IBoundCommand
{
    private readonly IExecutionContext _context;
    private readonly ICommand _command;

    internal BoundCommand(IExecutionContext context, ICommand command)
    {
        _context = context;
        _command = command;
    }

    public async Task<CommandResult> CaptureAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_command, CommandMode.Capture);

        var pipelineStep = new CommandPipelineStep(_command);
        var pipelineOutput = await pipelineStep.Connect(null, OutputMode.Capture, ct);
        var exitCode = await pipelineOutput.WaitForExit(ct);

        return await pipelineOutput.ToCommandResult(exitCode, ct);
    }

    public async Task<int> ExecAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_command, CommandMode.Run);

        var pipelineStep = new CommandPipelineStep(_command);
        var pipelineOutput = await pipelineStep.Connect(null, OutputMode.Capture, ct);
        var exitCode = await pipelineOutput.WaitForExit(ct);

        return exitCode;
    }

    public IBoundCommand ThrowOnError()
    {
        return new BoundErrorHandler(_context, this);
    }

    public override string ToString()
    {
        return _command.ToString()!;
    }
}
