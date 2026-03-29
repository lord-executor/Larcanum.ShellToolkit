using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit.SSH;

public class SshBoundPipeline : IBoundCommand
{
    private readonly ISshExecutionContext _context;
    private readonly IPipeline _pipeline;

    internal SshBoundPipeline(ISshExecutionContext context, IPipeline pipeline)
    {
        _context = context;
        _pipeline = pipeline;
    }

    public async Task<CommandResult> CaptureAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_pipeline, CommandMode.Capture);

        using var cmd = _context.Client.CreateCommand(_pipeline.ToString() ?? string.Empty);
        var task = cmd.ExecuteAsync(ct);
        await task;

        return new CommandResult
        {
            ExitCode = cmd.ExitStatus ?? _context.Settings.ProcessFailedExitCode,
            Output = cmd.Result,
            Error = cmd.Error,
        };
    }

    public async Task<int> ExecAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_pipeline, CommandMode.Run);

        using var cmd = _context.Client.CreateCommand(_pipeline.ToString() ?? string.Empty);
        var asyncResult = cmd.BeginExecute();

        var outputTask = Task.Run(() => {
            using var reader = new StreamReader(cmd.OutputStream);
            while (!asyncResult.IsCompleted || !reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null) Console.WriteLine(line);
            }
        }, ct);

        var errorTask = Task.Run(() => {
            using var reader = new StreamReader(cmd.ExtendedOutputStream);
            while (!asyncResult.IsCompleted || !reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null) Console.Error.WriteLine(line);
            }
        }, ct);

        await Task.Factory.FromAsync(asyncResult, cmd.EndExecute);
        await Task.WhenAll(outputTask, errorTask);

        return cmd.ExitStatus ?? _context.Settings.ProcessFailedExitCode;
    }

    public void ExecDetached()
    {
        _context.LogCommand(_pipeline, CommandMode.Detach);

        var cmd = _context.Client.CreateCommand(_pipeline.ToString() ?? string.Empty);
        cmd.BeginExecute();
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
