using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class ArgumentBinding<TArg, TProp> : ISymbolBinding<TArg>
    where TArg : IArguments<TArg>, new()
{
    private readonly Expression<Func<TArg, TProp>> _binding;
    private readonly Argument<TProp> _argument;

    public ArgumentBinding(Expression<Func<TArg, TProp>> binding, Argument<TProp> argument)
    {
        _binding = binding;
        _argument = argument;
    }

    public ArgumentBinding<TArg, TProp> WithValidator(Action<ArgumentResult> validator)
    {
        _argument.Validators.Add(validator);
        return this;
    }

    public ArgumentBinding<TArg, TProp> WithDefaultValue(TProp defaultValue)
    {
        _argument.DefaultValueFactory = _ => defaultValue;
        return this;
    }

    public ArgumentBinding<TArg, TProp> WithMultipleValues()
    {
        _argument.Arity = ArgumentArity.OneOrMore;
        return this;
    }

    public void AddToCommand(Command command)
    {
        command.Add(_argument);
    }

    public MemberAssignment? BindingExpression(ParameterExpression parseResultParam)
    {
        var prop = ExpressionUtils.GetPropertyInfo(_binding);
        Expression<Func<ParseResult, TProp>> valueExpression = r => r.GetRequiredValue(_argument);

        return Expression.Bind(prop, Expression.Invoke(valueExpression, parseResultParam));
    }
}
