using System.CommandLine;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public interface ISymbolBinding<in TArg>
    where TArg : IArguments<TArg>, new()
{
    void AddToCommand(Command command);
    MemberAssignment? BindingExpression(ParameterExpression parseResultParam);
}
