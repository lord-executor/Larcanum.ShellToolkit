using Renci.SshNet;

namespace Larcanum.ShellToolkit.SSH;

public interface ISshExecutionContext : IExecutionContext
{
    ISshClient Client { get; }
}
