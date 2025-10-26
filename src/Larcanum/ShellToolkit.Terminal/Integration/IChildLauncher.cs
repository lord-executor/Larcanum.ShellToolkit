namespace Larcanum.ShellToolkit.Terminal.Integration;

public interface IChildLauncher
{
    Task<int> RunAsync<TCommand, TArg>(CommandDefinition<TCommand, TArg> definition, TArg arg, CancellationToken ct)
        where TCommand : class, ICommand<TArg>
        where TArg : IArguments<TArg>, new();
}
