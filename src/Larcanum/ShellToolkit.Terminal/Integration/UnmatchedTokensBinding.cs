using System.CommandLine;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class UnmatchedTokensBinding<TArg> : ISymbolBinding<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly Expression<Func<TArg, IReadOnlyList<string>>> _binding;

    public UnmatchedTokensBinding(Expression<Func<TArg, IReadOnlyList<string>>> binding)
    {
        _binding = binding;
    }

    public void AddToCommand(Command command)
    {
        command.TreatUnmatchedTokensAsErrors = false;
    }

    public MemberAssignment? BindingExpression(ParameterExpression parseResultParam)
    {
        var prop = ExpressionUtils.GetPropertyInfo(_binding);
        Expression<Func<ParseResult, IReadOnlyList<string>>> valueExpression = r => r.UnmatchedTokens;

        return Expression.Bind(prop, Expression.Invoke(valueExpression, parseResultParam));
    }
}
