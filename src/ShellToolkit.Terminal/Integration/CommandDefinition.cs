using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Larcanum.ShellToolkit.Terminal.Integration;

/// <summary>
/// This class essentially serves as a "type definition", capturing the generic types in an instance so that we don't
/// have to provide them explicitly everywhere.
/// </summary>
public class CommandDefinition<TCommand, TArg> : ICommandDefinition
    where TCommand : class, ICommand<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly ArgumentFactory<TArg> _argFactory;

    public Command Command { get; }
    public List<IServiceModule> Modules { get; } = new List<IServiceModule>();

    public CommandDefinition(Command command)
    {
        Command = command;
        _argFactory = new ArgumentFactory<TArg>(Command);
    }

    public CommandDefinition(Command command, IEnumerable<IServiceModule> modules)
        : this(command)
    {
        Modules.AddRange(modules);
    }

    public async Task<int> RunAsync(IServiceProvider provider, ParseResult parseResult, CancellationToken ct = default)
    {
        var handler = provider.GetRequiredService<TCommand>();
        var args = _argFactory.Create(parseResult);
        return await handler.RunAsync(args, ct);
    }

    public async Task<int> RunAsync(IServiceProvider provider, TArg args, CancellationToken ct = default)
    {
        var handler = provider.GetRequiredService<TCommand>();
        return await handler.RunAsync(args, ct);
    }

    public static implicit operator CommandDefinition<TCommand, TArg>(Command command)
    {
        return new CommandDefinition<TCommand, TArg>(command);
    }
}
