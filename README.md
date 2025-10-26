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

TODO: Details and examples
