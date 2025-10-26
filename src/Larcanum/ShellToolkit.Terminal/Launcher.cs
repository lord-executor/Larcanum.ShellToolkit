using System.CommandLine;

using Larcanum.ShellToolkit.Terminal.Integration;
using Larcanum.ShellToolkit.Terminal.Rendering;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit.Terminal;

public class Launcher : IChildLauncher
{
    protected readonly ILauncherModule _launcherModule;
    protected readonly IConfiguration _config;
    protected readonly ServiceCollection _services;
    protected readonly CliLogger _logger;
    protected readonly HashSet<string> _loadedModules = new HashSet<string>();

    private ParseResult? _parseResult;
    private ServiceProvider? _provider;

    public Launcher(ILauncherModule launcherModule)
    {
        _launcherModule = launcherModule;
        _config = _launcherModule.GetConfiguration();

        _services = new ServiceCollection();
        _services.AddSingleton(_config);
        _services.AddSingleton<IChildLauncher>(this);

        _launcherModule.ConfigureRootServices(_services, _config);

        _logger = new CliLogger(StandardStreams.Default, LogLevel.Information, enableHostWriter: true);
        _services.AddSingleton<ICliLogger>(_logger);
    }

    public virtual Command Register<TCommand, TArg>(CommandDefinition<TCommand, TArg> definition)
        where TCommand : class, ICommand<TArg>
        where TArg : IArguments<TArg>, new()
    {
        var commandName = typeof(TCommand).FullName
                          ?? throw new ArgumentException("Command type must have a qualified name");

        _services.AddKeyedSingleton<ICommandDefinition>(commandName, definition);
        _services.AddScoped<TCommand>();
        _services.AddKeyedScoped<ICommand<TArg>, TCommand>(commandName);

        foreach (var mod in definition.Modules)
        {
            if (!_loadedModules.Contains(mod.Key))
            {
                mod.ConfigureServices(_services, _config);
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
            _launcherModule.ConfigureInvocationContext(_services, _config, _parseResult);
            return RunRootCommand(_parseResult, args);
        }
        catch (Exception e)
        {
            return Task.FromResult(OnException(e));
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
        _provider = _services.BuildServiceProvider();

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
                _logger.LogWarning($"The operation was aborted - {cEx.Message}");
                return 1;
            default:
                LogException(_logger, ex);
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
