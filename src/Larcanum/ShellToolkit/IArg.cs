namespace Larcanum.ShellToolkit;

public interface IArg
{
    string Argument { get; }
    string DisplayText { get; }
}

public record StringArg(string Argument) : IArg
{
    public string DisplayText => Argument;

    public static implicit operator StringArg(string arg) => new StringArg(arg);
}
