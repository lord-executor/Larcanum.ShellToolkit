using System.CommandLine;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public interface ICommandDefinition
{
    List<IServiceModule> Modules { get; }

    Task<int> RunAsync(IServiceProvider provider, ParseResult parseResult, CancellationToken ct = default);
}
