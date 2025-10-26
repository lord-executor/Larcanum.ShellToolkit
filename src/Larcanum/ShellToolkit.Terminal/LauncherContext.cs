using Larcanum.ShellToolkit.Terminal.Rendering;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Larcanum.ShellToolkit.Terminal;

public class LauncherContext
{
    public required IConfiguration Configuration { get; init; }
    public required ServiceCollection Services { get; init; }
    public required CliLogger Logger { get; init; }
}
