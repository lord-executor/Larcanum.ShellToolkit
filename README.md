# ShellToolkit
[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/lord-executor/Larcanum.ShellToolkit/blob/master/LICENSE) [![Nuget](https://img.shields.io/nuget/v/Larcanum.ShellToolkit.svg)](https://www.nuget.org/packages/Larcanum.ShellToolkit/)

The goal of this library is to provide a set of tools to start and interact with processes on the host system in a way that is similarly easy to use as _Bash_ or oder shells. First and foremost this means providing a convenient API on top of the rather crusty and awkward `System.Diagnostics.Process` and `System.Diagnostics.ProcessStartInfo`. On top of that, the library provides methods for building _pipelines_ of commands similar to how pipes work in Bash.

One common use case and the reason for creating this library is the creation of custom .NET command line tools that rely on other programs like `git`, `7z`, `dotnet`, `jq`, `sed`, `awk`, etc. to avoid reinventing the wheel.

# Examples

The examples here are assuming that the following command line tools are available in your system `PATH`
- [dotnet CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/)
- [jq](https://jqlang.github.io/jq/)

## Capturing and Processing simple Command Output

```cs
var runner = CommandRunner.Create();

var version = Version.Parse((await runner.CaptureAsync(Command.Create("dotnet", ["--version"]))).Output!);
// version.Major = 8
// version.Minor = 0
// version.Build = 200
```

## Running Command Output through Filters and into a File

```cs
var runner = CommandRunner.Create();
var tempFile = new FileInfo(Path.GetTempFileName());

var pipeline = Command.Create("dotnet", ["list", @"C:\path\to\ShellToolkit.csproj", "package", "--format", "json"])
    .Pipe(Command.Create("jq", [".projects.[].frameworks.[].topLevelPackages.[].id"]))
    .Pipe(tempFile);
 
if (await runner.ExecAsync(pipeline) != 0)
{
    throw new Exception("Pipeline failed");
}

var result = await File.ReadAllTextAsync(tempFile.FullName);
// result = "Microsoft.Extensions.Logging.Abstractions"
```

# ShellToolkit.Terminal
[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/lord-executor/Larcanum.ShellToolkit/blob/master/LICENSE) [![Nuget](https://img.shields.io/nuget/v/Larcanum.ShellToolkit.Terminal.svg)](https://www.nuget.org/packages/Larcanum.ShellToolkit.Terminal/)

This is a supplemental library for creating CLI programs based on the [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine) library from Microsoft.
Up until _beta4_ of that library in 2022, it came with a set of additional libraries to deal with common tasks for CLI programs like output rendering, argument binding, etc. Starting with _beta5_ in Summer of 2025, all of these additional libraries were deprecated and it is now up to other parties to help with that.

This is what _ShellToolkit.Terminal_ aims to do. Specifically, this library deals with these pain points of non-trivial CLI tools:

- Rendering colored output to the terminal and providing an injectable _logger_ abstraction
  - Based on `Microsoft.Extensions.Logging`
- Configuration management and dependency injection with an object-oriented command abstraction
  - Based on `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.DependencyInjection`
- Strongly typed binding to command arguments with simple classes

## CliLogger
The `CliLogger` is a lightweight, terminal-aware logger that implements `Microsoft.Extensions.Logging.ILogger` and adds a few CLI-specific conveniences.

- Colorized output by `LogLevel` using ANSI sequences (when supported):
  - `Trace` → light gray
  - `Debug` → light cyan
  - `Information` → light blue
  - `Warning` → yellow
  - `Error`/`Critical` → red.
- Three output targets:
  - `WriteOutput(...)` → STDOUT
  - `WriteError(...)` → STDERR
  - `WriteHost(...)` → prints directly to the console host when possible (bypasses STDOUT/STDERR). On Windows this uses the legacy `\\.\CON`, on
    Unix-like systems `/dev/tty`. If a host writer is not available it falls back to STDERR.
- Runtime control of verbosity via `SetLogLevel(LogLevel level)`.
- Simple prompting helpers: `Prompt(...)` and `PromptYesNo(...)`.

The `Launcher` wires a `CliLogger` into DI for you and exposes it as `ICliLogger` so that it can be injected into commands.

Typical usage inside a command handler:

```csharp
public class MyCommand : ICommand<MyArgs>
{
    private readonly ICliLogger _log;

    public MyCommand(ICliLogger log)
    {
        _log = log;
    }

    public Task<int> RunAsync(MyArgs args, CancellationToken ct = default)
    {
        _log.LogInformation("Starting work for {Path}", args.Path);

        if (!_log.PromptYesNo("Proceed?"))
        {
            _log.LogWarning("Aborted by user.");
            return Task.FromResult(1);
        }

        _log.LogContent("This goes to STDOUT as-is (no level prefix).\n");
        _log.WriteHost(new AnsiTextSpan("Host-only note", bold: true));
        return Task.FromResult(0);
    }
}
```

Use `LogContent(...)` for content that should be machine-consumable on STDOUT (e.g., JSON), keeping log messages on STDERR.

## Binding Arguments to Classes
Rather than binding options and arguments to method parameters, `ShellToolkit.Terminal` lets you bind them to a simple POCO that implements `IArguments<T>`.

- Define an arguments class: `public class MyArgs : IArguments<MyArgs>`
- Implement a single static method `Register(...)` that declares all bindings using `BindingBuilder<T>`.
- Supported bindings:
  - `BindOption(expr, name, description)` with fluent helpers:
  - `BindArgument(expr, name, description)`
  - `BindUnmatchedTokens(expr)` to capture extra tokens
  - `BindConfig(cmd => { ... })` to tweak the underlying `System.CommandLine.Command`

Example:

```csharp
public sealed class MyArgs : IArguments<MyArgs>
{
    public string? Path { get; set; }
    public bool Verbose { get; set; }
    public IReadOnlyList<string>? Extra { get; set; }

    public static IEnumerable<ISymbolBinding<MyArgs>> Register(BindingBuilder<MyArgs> b)
    {
        yield return b.BindOption(x => x.Path, "--path", "Path to something")
            .WithAlias("-p")
            .WithValidator(r =>
            {
                var v = r.GetValueForOption<string?>("--path");
                if (string.IsNullOrWhiteSpace(v))
                {
                    r.ErrorMessage = "--path must not be empty";
                }
            });

        yield return b.BindOption(x => x.Verbose, "--verbose", "Verbose output")
            .WithAlias("-v")
            .WithDefaultValue(false);

        yield return b.BindUnmatchedTokens(x => x.Extra!);

        // Optional low-level command tweaks
        yield return b.BindConfig(cmd => cmd.AddAlias("mi"));
    }
}
```

At runtime the `ArgumentFactory<T>` automatically:
- Adds declared symbols to the command instance
- Builds a `MyArgs` instance from the `ParseResult` using compiled expression trees (no reflection at invocation time)

## Strongly Typed Command
Commands are regular classes with DI and strongly typed args:

- Implement `ICommand<TArg>` with a single `RunAsync` method
- Describe your command via `CommandDefinition<TCommand, TArg>`
- Register with the `Launcher` and run

```csharp
public sealed class SampleCommand : ICommand<SampleArgs>
{
    private readonly ICliLogger _logger;
    public SampleCommand(ICliLogger logger) { _logger = logger; }
    public Task<int> RunAsync(SampleArgs args, CancellationToken ct = default)
    {
        _logger.LogInformation("Path = {Path}", args.Path);
        return Task.FromResult(0);
    }
}
```

## Example Program

```cs
var bootModule = new LauncherModule();
var launcher = new Launcher(bootModule);
var rootCommand = new RootCommand("Demo");

rootCommand.Add(launcher.Register(
    new CommandDefinition<SampleCommand, SampleArgs>(
        new Command("gitinfo", "Gathers information from git"))));

return await launcher.RunAsync(bootModule.RootCommand, args);

public class LauncherModule : ILauncherModule
{
    public Func<LauncherContext, Exception, int>? ExceptionHandler => null;

    public LauncherModule()
    {
    }

    public IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appSettings.json")
            .Build();
    }

    public void ConfigureRootServices(LauncherContext ctx)
    {
    }

    public void ConfigureInvocationContext(LauncherContext ctx, ParseResult parseResult)
    {
    }
}

public class SampleArgs : IArguments<SampleArgs>
{
    public string? Path { get; set; }

    public static IEnumerable<ISymbolBinding<SampleArgs>> Register(BindingBuilder<SampleArgs> builder)
    {
        yield return builder.BindOption(x => x.Path, "--path", "Path to something");
    }
}

public class SampleCommand : ICommand<GitInfoArgs>
{
    public static CommandDefinition<GitInfoCommand, GitInfoArgs> Def = new Command("gitinfo", "Gathers information from git");

    private readonly ICliLogger _logger;
    private readonly GitShellCommands _gitShellCommands;

    public SampleCommand(ICliLogger logger, GitShellCommands gitShellCommands)
    {
        _logger = logger;
        _gitShellCommands = gitShellCommands;
    }

    public async Task<int> RunAsync(GitInfoArgs args, CancellationToken ct = default)
    {
        var gitInfo = new Dictionary<string, string>
        {
            ["branch"] = await _gitShellCommands.CurrentBranch(args.Path, ct),
            ["commit"] = await _gitShellCommands.CurrentCommit(args.Path, ct),
            ["version"] = await _gitShellCommands.Describe(args.Path, ct)
        };

        _logger.LogContent(JsonSerializer.Serialize(gitInfo, new JsonSerializerOptions { WriteIndented = true }));

        return ExitCodes.Ok;
    }
}

```

See https://github.com/lord-executor/ModularCliTemplate for a proper example with additional context.
