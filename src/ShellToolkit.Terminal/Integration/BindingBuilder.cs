using System.CommandLine;
using System.Linq.Expressions;

namespace Larcanum.ShellToolkit.Terminal.Integration;

public class BindingBuilder<TArg>
    where TArg : IArguments<TArg>, new()
{
    public ArgumentBinding<TArg, TProp> BindArgument<TProp>(Expression<Func<TArg, TProp>> binding, string name, string description)
    {
        return new ArgumentBinding<TArg, TProp>(binding, new Argument<TProp>(name)
        {
            Description = description
        });
    }

    public OptionBinding<TArg, TProp> BindOption<TProp>(Expression<Func<TArg, TProp>> binding, string name,
        string description)
    {
        return new OptionBinding<TArg, TProp>(binding, new Option<TProp>(name)
        {
            Description = description
        });
    }

    public UnmatchedTokensBinding<TArg> BindUnmatchedTokens(Expression<Func<TArg, IReadOnlyList<string>>> binding)
    {
        return new UnmatchedTokensBinding<TArg>(binding);
    }

    public ConfigBinding<TArg> BindConfig(Action<Command> configure)
    {
        return new ConfigBinding<TArg>(configure);
    }
}
