using AwesomeAssertions;

using Larcanum.ShellToolkit;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ShellToolkit.UnitTests;

public class CommandRunnerTest
{
    private const string VersionRegex = @"\d+\.\d+\.\d+";

    [Fact]
    public async Task Execute_SingleValidCommand_ReturnsExitCode()
    {
        var runner = CreateRunner();
        var result = await runner.ExecAsync(SampleCommands.Version());

        result.Should().Be(0);
    }

    [Fact]
    public async Task Execute_SingleFailedCommand_ReturnsExitCode()
    {
        var runner = CreateRunner();
        var result = await runner.ExecAsync(SampleCommands.Fail());

        result.Should().Be(1);
    }

    [Fact]
    public async Task Execute_PipeToFile_ReturnsExitCode()
    {
        var runner = CreateRunner();
        using var tmp = new TempFile();
        var result = await runner.ExecAsync(SampleCommands.Version().Pipe(tmp.File));

        result.Should().Be(0);
        var output = await File.ReadAllTextAsync(tmp.File.FullName);
        output.Should().MatchRegex(VersionRegex);
    }

    [Fact]
    public async Task Execute_FailedPipeToFile_ReturnsExitCode()
    {
        var runner = CreateRunner();
        using var tmp = new TempFile();
        var result = await runner.ExecAsync(SampleCommands.Fail().Pipe(tmp.File));

        result.Should().Be(1);
        tmp.File.Exists.Should().BeTrue();
        tmp.File.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Capture_SingleValidCommand_ReturnsCommandResult()
    {
        var runner = CreateRunner();
        var result = await runner.CaptureAsync(SampleCommands.Version());
        result.ExitCode.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
        result.Output.Should().MatchRegex(VersionRegex);
        result.Error.Should().Be("");
    }

    [Fact]
    public async Task Capture_SingleFailedCommand_ReturnsCommandResult()
    {
        var runner = CreateRunner();
        var result = await runner.CaptureAsync(SampleCommands.Fail());
        result.ExitCode.Should().Be(1);
        result.IsSuccess.Should().BeFalse();
        result.Output.Should().Contain("Possible reasons for this include");
        result.Error.Should().Contain("Could not execute because the specified command or file was not found");
    }

    [Fact]
    public void ExecuteAsync_WithoutAwait_NoResponse()
    {
        var runner = CreateRunner();
        runner.ExecAsync(SampleCommands.Version());

        runner.Should().NotBeNull();
    }

    private CommandRunner CreateRunner()
    {
        return new CommandRunner(new Settings(), new NullLogger<CommandRunner>());
    }

    private static class SampleCommands
    {
        public const string CommandName = "dotnet";

        public static Command Version()
        {
            return Command.Create(CommandName, ["--version"]);
        }

        public static Command Fail()
        {
            return Command.Create(CommandName, ["fail"]);
        }
    }
}
