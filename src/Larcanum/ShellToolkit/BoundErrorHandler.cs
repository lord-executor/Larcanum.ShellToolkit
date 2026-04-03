using System.Text;

namespace Larcanum.ShellToolkit;

public class BoundErrorHandler : IBoundCommand
{
    private readonly IExecutionContext _context;
    private readonly IBoundCommand _inner;

    public BoundErrorHandler(IExecutionContext context, IBoundCommand inner)
    {
        _context = context;
        _inner = inner;
    }

    public async Task<CommandResult> CaptureAsync(CancellationToken ct = default)
    {
        return CheckAndThrow(await _inner.CaptureAsync(ct), _inner);
    }

    public async Task<int> ExecAsync(CancellationToken ct = default)
    {
        return CheckAndThrow(await _inner.ExecAsync(ct), _inner);
    }

    public IBoundCommand ThrowOnError()
    {
        return this;
    }

    private CommandResult CheckAndThrow(CommandResult result, object cmd)
    {
        CheckAndThrow(result.ExitCode, cmd, result.Error);
        return result;
    }

    private int CheckAndThrow(int exitCode, object cmd, string? errorMsg = null)
    {
        if (exitCode != 0)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Command '{cmd}' failed with exit code: {exitCode}");
            if (errorMsg != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append(errorMsg);
            }
            throw new CommandFailureException(_context.Settings.ProcessFailedExitCode, messageBuilder.ToString());
        }
        return exitCode;
    }
}
