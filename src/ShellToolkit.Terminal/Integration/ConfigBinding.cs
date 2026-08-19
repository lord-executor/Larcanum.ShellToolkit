using System.CommandLine;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class ConfigBinding<TArg> : ISymbolBinding<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly Action<Command> _configure;

    public ConfigBinding(Action<Command> configure)
    {
        _configure = configure;
    }

    public void AddToCommand(Command command)
    {
        _configure(command);
    }

    public MemberAssignment? BindingExpression(ParameterExpression parseResultParam)
    {
        return null;
    }
}
