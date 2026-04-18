using System.Diagnostics;
using System.Text;

using AwesomeAssertions;

using Larcanum.ShellToolkit;

using Xunit;

namespace ShellToolkit.UnitTests;

public class FileSinkPipelineStepTest
{
    [Fact]
    public async Task Connect_UsingOutputModeCapture_ThrowsException()
    {
        using var f = WithTempFile();
        var sink = new FileSinkPipelineStep(f.File);
        Func<Task> action = async () => await sink.Connect(new StreamPipelineOutput(Stream.Null), OutputMode.Capture);
        (await action.Should().ThrowAsync<InvalidOperationException>())
            .And.Message.Should().Contain("Cannot capture output");
    }

    [Fact]
    public async Task Connect_ToPreviousWithoutOutput_ThrowsException()
    {
        using var f = WithTempFile();
        var sink = new FileSinkPipelineStep(f.File);

        Func<Task> action = async () => await sink.Connect(null, OutputMode.Default);
        (await action.Should().ThrowAsync<InvalidOperationException>())
            .And.Message.Should().Contain("not provide a connectable output");
    }

    [Fact]
    public async Task Connect_ToPreviousWithOutput_CopiesOutputToFile()
    {
        var data = "This is\nsample output\nwith 3 lines";
        using var f = WithTempFile();
        var sink = new FileSinkPipelineStep(f.File);
        var stream = new MemoryStream();
        stream.Write(Encoding.UTF8.GetBytes(data));
        stream.Seek(0, SeekOrigin.Begin);
        var previous = new StreamPipelineOutput(stream);

        var result = await sink.Connect(previous, OutputMode.Default);
        var exitCode = await result.WaitForExit();

        exitCode.Should().Be(0);
        result.Out.Length.Should().Be(0);
        result.Error.Length.Should().Be(0);

        var fileContent = await File.ReadAllTextAsync(f.File.FullName);
        fileContent.Should().Be(data);
    }

    private TempFile WithTempFile()
    {
        return new TempFile();
    }
}
