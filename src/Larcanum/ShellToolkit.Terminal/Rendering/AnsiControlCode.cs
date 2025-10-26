namespace Larcanum.ShellToolkit.Terminal.Rendering;

public record AnsiControlCode(string EscapeSequence)
{
    public static AnsiControlCode ForegroundReset = Ansi.Color.Foreground.Default;
    public static AnsiControlCode BackgroundReset = Ansi.Color.Background.Default;

    public void WriteTo(TextWriter writer)
    {
        writer.Write(EscapeSequence);
    }

    public static implicit operator AnsiControlCode(string sequence)
    {
        return new AnsiControlCode(sequence);
    }
}
