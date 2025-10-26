using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class OptionBinding<TArg, TProp> : ISymbolBinding<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly Expression<Func<TArg, TProp>> _binding;
    private readonly Option<TProp> _option;

    public OptionBinding(Expression<Func<TArg, TProp>> binding, Option<TProp> option)
    {
        _binding = binding;
        _option = option;
    }

    public OptionBinding<TArg, TProp> WithValidator(Action<OptionResult> validator)
    {
        _option.Validators.Add(validator);
        return this;
    }

    public OptionBinding<TArg, TProp> WithAlias(string alias)
    {
        _option.Aliases.Add(alias);
        return this;
    }

    public OptionBinding<TArg, TProp> WithDefaultValue(TProp defaultValue)
    {
        _option.DefaultValueFactory = _ => defaultValue;
        return this;
    }

    public OptionBinding<TArg, TProp> WithMultipleValues()
    {
        _option.Arity = ArgumentArity.OneOrMore;
        _option.AllowMultipleArgumentsPerToken = true;
        return this;
    }

    public void AddToCommand(Command command)
    {
        command.Add(_option);
    }

    public MemberAssignment? BindingExpression(ParameterExpression parseResultParam)
    {
        var prop = ExpressionUtils.GetPropertyInfo(_binding);
        Expression<Func<ParseResult, TProp>> valueExpression = r => r.GetValue(_option)!;

        return Expression.Bind(prop, Expression.Invoke(valueExpression, parseResultParam));
    }
}
