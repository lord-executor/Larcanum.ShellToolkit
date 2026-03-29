using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Larcanum.ShellToolkit;

public class CommandRunner : ICommandRunner, IExecutionContext
{
    public static CommandRunner Create()
    {
        return new CommandRunner(new Settings(), new NullLogger<CommandRunner>());
    }

    public static CommandRunner Create(Settings settings)
    {
        return new CommandRunner(settings, new NullLogger<CommandRunner>());
    }

    public static CommandRunner Create(ILogger<CommandRunner> logger)
    {
        return new CommandRunner(new Settings(), logger);
    }

    private readonly Settings _settings;
    private readonly ILogger _logger;

    Settings IExecutionContext.Settings => _settings;
    ILogger IExecutionContext.Logger => _logger;

    void IExecutionContext.LogCommand(object command, CommandMode mode)
    {
        _logger.LogDebug("[exec{m}]: {cmd}", mode.AsString(), command);
    }

    public CommandRunner(Settings settings, ILogger<CommandRunner> logger)
        : this(settings, (ILogger)logger)
    {
    }

    public CommandRunner(Settings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public IBoundCommand Bind(ICommand command)
    {
        return new BoundCommand(this, command);
    }

    public IBoundCommand Bind(IPipeline pipeline)
    {
        return new BoundPipeline(this, pipeline);
    }

    public Task<int> ExecAsync(ICommand cmd, CancellationToken ct = default)
    {
        return Bind(cmd).ExecAsync(ct);
    }

    public Task<int> ExecAsync(IPipeline pipeline, CancellationToken ct = default)
    {
        return Bind(pipeline).ExecAsync(ct);
    }

    public Task<CommandResult> CaptureAsync(ICommand cmd, CancellationToken ct = default)
    {
        return Bind(cmd).CaptureAsync(ct);
    }

    public Task<CommandResult> CaptureAsync(IPipeline pipeline, CancellationToken ct = default)
    {
        return Bind(pipeline).CaptureAsync(ct);
    }

    public void ExecDetached(ICommand cmd)
    {
        Bind(cmd).ExecDetached();
    }

    public void ExecDetached(IPipeline pipeline)
    {
        Bind(pipeline).ExecDetached();
    }
}
