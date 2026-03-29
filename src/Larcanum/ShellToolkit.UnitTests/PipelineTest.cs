using Larcanum.ShellToolkit;
using AwesomeAssertions;
using Xunit;

namespace ShellToolkit.UnitTests;

public class PipelineTest
{
    [Fact]
    public void Pipeline_ToString_ReturnsCorrectString()
    {
        var cmd1 = Command.Create("echo hello");
        var cmd2 = Command.Create("grep hello");
        var file = new FileInfo("output.txt");

        var pipeline = cmd1.Pipe(cmd2).Pipe(file);

        // CommandPipelineStep returns " | {cmd}"
        // FileSinkPipelineStep returns " > {file.FullName}"
        pipeline.ToString().Should().Be($"echo hello | grep hello > {file.FullName}");
    }

    [Fact]
    public void Pipeline_Composition_CreatesCorrectStructure()
    {
        var cmd1 = Command.Create("cmd1");
        var cmd2 = Command.Create("cmd2");

        var pipeline = cmd1.Pipe(cmd2);

        pipeline.Should().NotBeNull();
        pipeline.ToString().Should().Be("cmd1 | cmd2");
    }

    [Fact]
    public void Pipeline_MultipleSteps_ToString()
    {
        var file = new FileInfo("result.log");
        var pipeline = Command.Create("cat", ["file.txt"])
            .Pipe(Command.Create("grep", ["pattern"]))
            .Pipe(Command.Create("sort"))
            .Pipe(file);

        pipeline.ToString().Should().Be($"cat file.txt | grep pattern | sort > {file.FullName}");
    }
}
