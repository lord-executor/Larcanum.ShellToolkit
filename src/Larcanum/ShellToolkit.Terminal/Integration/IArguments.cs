namespace Larcanum.ShellToolkit.Terminal.Integration;

public interface IArguments<TArg>
    where TArg : IArguments<TArg>, new()
{
    static abstract IEnumerable<ISymbolBinding<TArg>> Register(BindingBuilder<TArg> builder);
}
