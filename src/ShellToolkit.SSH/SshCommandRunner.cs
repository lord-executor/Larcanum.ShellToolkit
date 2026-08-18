using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Renci.SshNet;

namespace Larcanum.ShellToolkit.SSH;

public class SshCommandRunner : ICommandRunner, IExecutionContext
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

    Settings IExecutionContext.Settings => _settings;
    ILogger IExecutionContext.Logger => _logger;

    void IExecutionContext.LogCommand(object command, CommandMode mode)
    {
        _logger.LogDebug("[sshex{m}@{host}]: {cmd}", mode.AsString(), _client.ConnectionInfo.Host, command);
    }

    IPipelineStep IExecutionContext.CreatePipelineStep(ICommand command)
    {
        return new SshPipelineStep(_client, command);
    }

    public IBoundCommand Bind(IPipeline pipeline)
    {
        return new BoundPipeline(this, pipeline);
    }
}
