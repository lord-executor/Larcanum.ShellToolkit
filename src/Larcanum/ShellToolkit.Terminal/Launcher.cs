using System.CommandLine;

using Larcanum.ShellToolkit.Terminal.Integration;
using Larcanum.ShellToolkit.Terminal.Rendering;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit.Terminal;

public class Launcher : IChildLauncher
{
    protected readonly LauncherContext _context;
    protected readonly ILauncherModule _launcherModule;
    protected readonly HashSet<string> _loadedModules = new HashSet<string>();

    private ParseResult? _parseResult;
    private ServiceProvider? _provider;

    public Launcher(ILauncherModule launcherModule)
    {
        _launcherModule = launcherModule;

        _context = new LauncherContext()
        {
            Configuration = _launcherModule.GetConfiguration(),
            Services = new ServiceCollection(),
            Logger = new CliLogger(StandardStreams.Default, LogLevel.Information, enableHostWriter: true)
        };

        _context.Services.AddSingleton(_context.Configuration);
        _context.Services.AddSingleton<IChildLauncher>(this);

        _launcherModule.ConfigureRootServices(_context);

        _context.Services.AddSingleton<ICliLogger>(_context.Logger);
    }

    public virtual Command Register<TCommand, TArg>(CommandDefinition<TCommand, TArg> definition)
        where TCommand : class, ICommand<TArg>
        where TArg : IArguments<TArg>, new()
    {
        var commandName = typeof(TCommand).FullName
                          ?? throw new ArgumentException("Command type must have a qualified name");

        _context.Services.AddKeyedSingleton<ICommandDefinition>(commandName, definition);
        _context.Services.AddScoped<TCommand>();
        _context.Services.AddKeyedScoped<ICommand<TArg>, TCommand>(commandName);

        foreach (var mod in definition.Modules)
        {
            if (!_loadedModules.Contains(mod.Key))
            {
                mod.ConfigureServices(_context.Services, _context.Configuration);
                _loadedModules.Add(mod.Key);
            }
        }

        var cmd = definition.Command;
        cmd.SetAction(CommandAction<TCommand, TArg>(definition));

        return cmd;
    }

    public virtual Task<int> RunAsync(RootCommand rootCommand, string[] args)
    {
        try
        {
            _parseResult = rootCommand.Parse(args);
            _launcherModule.ConfigureInvocationContext(_context, _parseResult);
            return RunRootCommand(_parseResult, args);
        }
        catch (Exception e)
        {
            return Task.FromResult(_launcherModule.ExceptionHandler?.Invoke(e) ?? OnException(e));
        }
    }

    public virtual async Task<int> RunAsync<TCommand, TArg>(CommandDefinition<TCommand, TArg> definition, TArg arg, CancellationToken ct)
        where TCommand : class, ICommand<TArg>
        where TArg : IArguments<TArg>, new()
    {
        using var scope = _provider!.CreateScope();
        return await definition.RunAsync(scope.ServiceProvider, arg, ct);
    }

    protected virtual Func<ParseResult, CancellationToken, Task<int>> CommandAction<TCommand, TArg>(ICommandDefinition definition)
    {
        return async (parseResult, ct) =>
        {
            using var scope = _provider!.CreateScope();
            return await definition.RunAsync(scope.ServiceProvider, parseResult, ct);
        };
    }

    protected virtual async Task<int> RunRootCommand(ParseResult parseResult, string[] args)
    {
        _provider = _context.Services.BuildServiceProvider();

        return await parseResult.InvokeAsync(new InvocationConfiguration()
        {
            EnableDefaultExceptionHandler = false,
        });
    }

    protected virtual int OnException(Exception ex)
    {
        switch (ex)
        {
            case OperationCanceledException cEx:
                _context.Logger.LogWarning($"The operation was aborted - {cEx.Message}");
                return 1;
            default:
                LogException(_context.Logger, ex);
                return 1;
        }
    }

    protected virtual void LogException(ICliLogger logger, Exception e)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogError(e, e.ToString());
        }
        else if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError(e.Message);
        }
    }
}
