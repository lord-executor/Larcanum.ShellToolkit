namespace Larcanum.ShellToolkit.Terminal.Rendering;

public record StandardStreams(TextReader Input, TextWriter Output, TextWriter Error)
{
    public static StandardStreams Default { get; } = new StandardStreams(Console.In, Console.Out, Console.Error);
}
