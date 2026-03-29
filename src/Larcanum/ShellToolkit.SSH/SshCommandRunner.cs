using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Renci.SshNet;

namespace Larcanum.ShellToolkit.SSH;

public class SshCommandRunner : ICommandRunner, ISshExecutionContext
{
    public static SshCommandRunner Create(ISshClient client)
    {
        return new SshCommandRunner(client, new Settings(), new NullLogger<SshCommandRunner>());
    }

    public static SshCommandRunner Create(ISshClient client, Settings settings)
    {
        return new SshCommandRunner(client, settings, new NullLogger<SshCommandRunner>());
    }

    public static SshCommandRunner Create(ISshClient client, ILogger<SshCommandRunner> logger)
    {
        return new SshCommandRunner(client, new Settings(), logger);
    }

    private readonly ISshClient _client;
    private readonly Settings _settings;
    private readonly ILogger _logger;

    ISshClient ISshExecutionContext.Client => _client;
    Settings IExecutionContext.Settings => _settings;
    ILogger IExecutionContext.Logger => _logger;

    void IExecutionContext.LogCommand(object command, CommandMode mode)
    {
        _logger.LogDebug("[sshex{m}@{host}]: {cmd}", mode.AsString(), _client.ConnectionInfo.Host, command);
    }

    public SshCommandRunner(ISshClient client, Settings settings, ILogger<SshCommandRunner> logger)
        : this(client, settings, (ILogger)logger)
    {
    }

    public SshCommandRunner(ISshClient client, Settings settings, ILogger logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
    }

    public IBoundCommand Bind(ICommand command)
    {
        return new SshBoundCommand(this, command);
    }

    public IBoundCommand Bind(IPipeline pipeline)
    {
        return new SshBoundPipeline(this, pipeline);
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
