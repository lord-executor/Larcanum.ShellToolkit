using System.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Larcanum.ShellToolkit.Terminal;

public interface ILauncherModule
{
    IConfiguration GetConfiguration();
    void ConfigureRootServices(IServiceCollection services, IConfiguration config);
    void ConfigureInvocationContext(IServiceCollection services, IConfiguration config, ParseResult parseResult);
}
