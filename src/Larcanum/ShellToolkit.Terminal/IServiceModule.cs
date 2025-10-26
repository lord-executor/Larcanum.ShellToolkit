using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Larcanum.ShellToolkit.Terminal;

public interface IServiceModule
{
    string Key => GetType().FullName!;
    void ConfigureServices(IServiceCollection services, IConfiguration config);
}
