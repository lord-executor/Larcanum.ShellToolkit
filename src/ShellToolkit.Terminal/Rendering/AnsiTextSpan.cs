namespace Larcanum.ShellToolkit.Terminal.Rendering;

public class AnsiTextSpan
{
    private readonly string _text;
    private readonly bool _newLine;
    private AnsiControlCode? _foregroundColor;
    private AnsiControlCode? _backgroundColor;

    public AnsiTextSpan(string text, bool newLine = false)
    {
        _text = text;
        _newLine = newLine;
    }

    public AnsiTextSpan WithForegroundColor(AnsiControlCode color)
    {
        _foregroundColor = color;
        return this;
    }

    public AnsiTextSpan WithBackgroundColor(AnsiControlCode color)
    {
        _backgroundColor = color;
        return this;
    }

    public void WriteTo(TextWriter writer, AnsiOutputMode mode)
    {
        switch (mode)
        {
            case AnsiOutputMode.Ansi:
                _foregroundColor?.WriteTo(writer);
                _backgroundColor?.WriteTo(writer);
                writer.Write(_text);
                (_foregroundColor is null ? null : AnsiControlCode.ForegroundReset)?.WriteTo(writer);
                (_backgroundColor is null ? null : AnsiControlCode.BackgroundReset)?.WriteTo(writer);
                break;
            default:
                writer.Write(_text);
                break;
        }

        if (_newLine)
        {
            writer.WriteLine();
        }
    }
}
