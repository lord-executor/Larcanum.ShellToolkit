using System.CommandLine;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class ArgumentFactory<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly IList<ISymbolBinding<TArg>> _bindings;

    public ArgumentFactory(Command command)
    {
        _bindings = TArg.Register(new BindingBuilder<TArg>()).ToList();

        foreach (var binding in _bindings)
        {
            binding.AddToCommand(command);
        }
    }

    public TArg Create(ParseResult parseResult)
    {
        var parseResultParam = Expression.Parameter(typeof(ParseResult), "parseResult");
        var bindingExpressions = _bindings.Select(b => b.BindingExpression(parseResultParam) as MemberBinding)
            .Where(b => b != null)
            .Cast<MemberBinding>()
            .ToArray();

        var body = Expression.MemberInit(Expression.New(typeof(TArg)), bindingExpressions);
        var exp = Expression.Lambda<Func<ParseResult, TArg>>(body, parseResultParam);
        return exp.Compile()(parseResult);
    }
}
