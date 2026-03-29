using AwesomeAssertions;

using Larcanum.ShellToolkit;

using Xunit;

namespace ShellToolkit.UnitTests;

public class CommandTest
{
    [Fact]
    public void Create_WithEnumerableArgs_PopulatesArgumentList()
    {
        var cmd = Command.Create("test", ["arg1", "arg2", "arg 3"]);

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("test");
        psi.ArgumentList.Should().ContainInOrder(["arg1", "arg2", "arg 3"]);
    }

    [Fact]
    public void Create_WithStringArgs_PopulatesArgumentList()
    {
        var cmd = Command.Create("test", "arg1 arg2 arg 3");

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("test");
        psi.ArgumentList.Should().ContainInOrder(["arg1", "arg2", "arg", "3"]);
    }

    [Fact]
    public void Create_FromSingleString_PopulatesArgumentList()
    {
        var cmd = Command.Create("test arg1 arg2 arg 3");

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("test");
        psi.ArgumentList.Should().ContainInOrder(["arg1", "arg2", "arg", "3"]);
    }

    [Fact]
    public void Create_FromStringWithQuotedArgs_RespectsQuotes()
    {
        var cmd = Command.Create("echo \"Hello World\" some\\\"thing \"Foo \\\"with\\\" Bar\"");

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("echo");
        psi.ArgumentList.Should().ContainInOrder(["Hello World", "some\"thing", "Foo \"with\" Bar"]);
    }

    [Fact]
    public void Create_FromStringOtherEscapeSequence_PreservesEscapeSequences()
    {
        var cmd = Command.Create("echo Hello\\-World \"Foo \\\\ Bar\"");

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("echo");
        psi.ArgumentList.Should().ContainInOrder(["Hello\\-World", "Foo \\\\ Bar"]);
    }

    [Fact]
    public void Create_FromStringWithQuotedPath_PreservesPath()
    {
        var cmd = Command.Create("cmd.exe /c dir \"C:\\Program Files\\dotnet\"");

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("cmd.exe");
        psi.ArgumentList.Should().ContainInOrder(["/c", @"C:\Program Files\dotnet"]);
    }

    [Fact]
    public void Create_WithVariousArgTypes_PopulatesArgumentList()
    {
        var args = new IArg[] { new StringArg("arg1"), (StringArg)"arg2" };
        var cmd = Command.Create("test", args);

        var psi = cmd.ToProcessStartInfo();
        psi.ArgumentList.Should().ContainInOrder(["arg1", "arg2"]);
    }

    [Fact]
    public void ToString_IncludesArguments()
    {
        var cmd = Command.Create("echo", ["hello", "world"]);
        cmd.ToString().Should().Be("echo hello world");
    }

    [Fact]
    public void Create_SingleStringWithMixedQuotes_ParsesCorrectly()
    {
        var cmd = Command.Create("sh -c \"ls -l | grep 'foo'\"");
        var psi = cmd.ToProcessStartInfo();

        psi.FileName.Should().Be("sh");
        psi.ArgumentList.Should().ContainInOrder(["-c", "ls -l | grep 'foo'"]);
    }

    [Fact]
    public void Create_SingleStringWithEscapedQuotes_ParsesCorrectly()
    {
        var cmd = Command.Create(@"echo ""He said \""Hello\""""");
        var psi = cmd.ToProcessStartInfo();

        psi.FileName.Should().Be("echo");
        psi.ArgumentList.Should().ContainInOrder(["He said \"Hello\""]);
    }
}
