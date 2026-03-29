namespace Larcanum.ShellToolkit.SSH;

public class SshBoundCommand : IBoundCommand
{
    private readonly ISshExecutionContext _context;
    private readonly ICommand _command;

    internal SshBoundCommand(ISshExecutionContext context, ICommand command)
    {
        _context = context;
        _command = command;
    }

    public async Task<CommandResult> CaptureAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_command, CommandMode.Capture);

        using var cmd = _context.Client.CreateCommand(_command.ToString() ?? string.Empty);
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
        _context.LogCommand(_command, CommandMode.Run);

        // For "interactive" execution over SSH, we might want to use ShellStream,
        // but for an analogous implementation to CommandRunner's ExecAsync (which waits for exit),
        // SshCommand.Execute is often sufficient if we don't need full terminal interactivity.
        // However, SSH.NET doesn't easily "forward" the local console to the remote process
        // in the same way Process.Start(info) does locally when CreateNoWindow = false.

        using var cmd = _context.Client.CreateCommand(_command.ToString() ?? string.Empty);
        // We want to see the output in real-time if it's "ExecAsync" (analogous to local ExecAsync)
        // CommandRunner.ExecAsync doesn't redirect output, so it just goes to the console.
        // For SSH, we need to explicitly read and write to console if we want that behavior.

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
        _context.LogCommand(_command, CommandMode.Detach);

        var cmd = _context.Client.CreateCommand(_command.ToString() ?? string.Empty);
        cmd.BeginExecute();
        // Note: we are not disposing cmd here because it needs to run in background.
        // This might leak if not careful, but SSH.NET's SshCommand doesn't have a
        // direct "fire and forget and forget about the object" mode that is safe.
        // In local CommandRunner, Process.Start(info) returns a Process object that
        // we don't dispose in ExecDetached.
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
