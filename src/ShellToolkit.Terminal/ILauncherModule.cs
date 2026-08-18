using System.CommandLine;

using Microsoft.Extensions.Configuration;

namespace Larcanum.ShellToolkit.Terminal;

public interface ILauncherModule
{
    public Func<LauncherContext, Exception, int>? ExceptionHandler { get; }

    IConfiguration GetConfiguration();
    void ConfigureRootServices(LauncherContext ctx);
    void ConfigureInvocationContext(LauncherContext ctx, ParseResult parseResult);
}
