using AwesomeAssertions;

using Larcanum.ShellToolkit;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ShellToolkit.UnitTests;

public class BoundCommandTest
{
    [Fact]
    public async Task HelpCommand_CaptureAsString_ReturnsHelpText()
    {
        var tool = new SampleTool();
        var result = await tool.Help().CaptureAsStringAsync();

        result.Should().Contain("Execute a .NET application");
    }

    [Fact]
    public async Task ListCommand_CaptureAsString_ThrowsException()
    {
        var tool = new SampleTool();
        Func<Task<string>> call = () => tool.List().CaptureAsStringAsync();

        (await call.Should().ThrowAsync<CommandFailureException>()).Which.ExitCode.Should().Be(500);
    }

    [Fact]
    public async Task CheckSdkCommand_Capture_ReturnsUnprocessedCommandResult()
    {
        var tool = new SampleTool();
        var result = await tool.CheckSdk().CaptureAsync();

        result.ExitCode.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
        result.Output.Should().Contain(".NET SDKs");
    }

    private class SampleTool
    {
        private const string CommandName = "dotnet";
        private readonly ICommandRunner _runner;

        public SampleTool()
        {
            _runner = new CommandRunner(new Settings(), new NullLogger<CommandRunner>());
        }

        public IBoundCommand Help()
        {
            return _runner.Bind(Command.Create(CommandName, ["help"]));
        }

        public IBoundCommand List()
        {
            return _runner.Bind(Command.Create(CommandName, ["list"]));
        }

        public IBoundCommand CheckSdk()
        {
            return _runner.Bind(Command.Create(CommandName, ["sdk", "check"]));
        }
    }
}
