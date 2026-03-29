using System.Diagnostics;

using Microsoft.Extensions.Logging;

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

        var output = new StringWriter();
        var error = new StringWriter();
        using var p = await RunRedirected(_command, msg => output.WriteLine(msg), msg => error.WriteLine(msg), ct);

        return new CommandResult
        {
            ExitCode = p.ExitCode,
            Output = output.ToString(),
            Error = error.ToString(),
        };
    }

    public Task<int> ExecAsync(CancellationToken ct = default)
    {
        _context.LogCommand(_command, CommandMode.Run);

        var info = _command.ToProcessStartInfo();
        info.UseShellExecute = false;
        // This is important for "forwarding" access to the console to the child process and even though
        // false is the default value, we want to make this very explicit here. This allows tools with fancy console
        // UI like menus and progress bars to work when created as child processes.
        info.CreateNoWindow = false;

        using var process = Process.Start(info);
        process?.WaitForExit();

        return Task.FromResult(process?.ExitCode ?? _context.Settings.NoProcessSpawnedExitCode);
    }

    public void ExecDetached()
    {
        _context.LogCommand(_command, CommandMode.Detach);

        var info = _command.ToProcessStartInfo();
        info.UseShellExecute = false;

        Process.Start(info);
    }

    public IBoundCommand ThrowOnError()
    {
        return new BoundErrorHandler(_context, this);
    }

    public override string ToString()
    {
        return _command.ToString()!;
    }

    private async Task<Process> RunRedirected(ICommand cmd, Action<string> output, Action<string> error, CancellationToken ct)
    {
        var info = cmd.ToProcessStartInfo();
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;

        var p = new Process() { StartInfo = info };
        p.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data != null)
            {
                output(eventArgs.Data!);
            }
        };
        p.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data != null)
            {
                error(eventArgs.Data!);
            }
        };

        p.Start();

        p.BeginErrorReadLine();
        p.BeginOutputReadLine();

        await p.WaitForExitAsync(ct);

        return p;
    }
}
