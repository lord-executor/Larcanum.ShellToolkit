namespace Larcanum.ShellToolkit.Terminal.Integration;

public interface ICommand<in TArg>
    where TArg : IArguments<TArg>, new()
{
    Task<int> RunAsync(TArg args, CancellationToken ct = default);
}
