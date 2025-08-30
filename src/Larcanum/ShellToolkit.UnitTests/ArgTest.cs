using FluentAssertions;

using Larcanum.ShellToolkit;

using Xunit;

namespace ShellToolkit.UnitTests;

public class ArgTest
{
    [Fact]
    public void StringArg_FromStringCast_CreatesStringArg()
    {
        string str = "foo";
        StringArg arg = str;

        arg.Argument.Should().Be(str);
        arg.DisplayText.Should().Be(str);

        var arg2 = (StringArg)str;
        arg2.Argument.Should().Be(str);
        arg2.DisplayText.Should().Be(str);
    }

    [Fact]
    public void Command_FromLongString_ParsesStringArgs()
    {
        var cmdStr = "git log --graph --all --decorate";
        var cmd = Command.Create(cmdStr);

        var psi = cmd.ToProcessStartInfo();
        psi.FileName.Should().Be("git");
        psi.ArgumentList.Should().ContainInOrder("log", "--graph", "--all", "--decorate");

        cmd.ToString().Should().Be(cmdStr);
    }

    [Fact]
    public void Command_WithRedactedArg_DisplaysRedactedForm()
    {
        var cmd = Command.Create("echo", [(StringArg)"first", new RedactedArg("second"), (StringArg)"third"]);
        cmd.ToString().Should().Be("echo first *** third");
    }

    private record RedactedArg(string Argument) : IArg
    {
        public string DisplayText => "***";
    }
}
