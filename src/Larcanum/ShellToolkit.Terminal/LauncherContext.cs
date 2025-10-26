using Larcanum.ShellToolkit.Terminal.Rendering;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Larcanum.ShellToolkit.Terminal;

public class LauncherContext
{
    public IConfiguration Configuration { get; init; }
    public ServiceCollection Services { get; init; }
    public CliLogger Logger { get; init; }
}
