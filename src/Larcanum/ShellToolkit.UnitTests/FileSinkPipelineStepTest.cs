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
        Func<Task> action = async () => await sink.Connect(new PipelineOutput(StreamReader.Null), OutputMode.Capture);
        (await action.Should().ThrowAsync<InvalidOperationException>())
            .And.Message.Should().Contain("Cannot capture output");
    }

    [Fact]
    public async Task Connect_ToPreviousWithoutOutput_ThrowsException()
    {
        using var f = WithTempFile();
        var sink = new FileSinkPipelineStep(f.File);
        var proc = new Process() { StartInfo = new ProcessStartInfo() { FileName = "foobar" } };
        var previous = new PipelineOutput(proc);

        Func<Task> action = async () => await sink.Connect(previous, OutputMode.Default);
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
        var previous = new PipelineOutput(new StreamReader(stream));

        var result = await sink.Connect(previous, OutputMode.Default);

        result.Should().BeSameAs(PipelineOutput.Empty);

        var fileContent = await File.ReadAllTextAsync(f.File.FullName);
        fileContent.Should().Be(data);
    }

    private TempFile WithTempFile()
    {
        return new TempFile();
    }
}
