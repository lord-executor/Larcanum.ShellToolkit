using System.Diagnostics;

namespace Larcanum.ShellToolkit.Terminal.Rendering;

public static class Ansi
{
    public static string Esc { get; } = "\u001b";

    [DebuggerStepThrough]
    public static class Color
    {
        [DebuggerStepThrough]
        public static class Background
        {
            public static AnsiControlCode Default { get; } = $"{Esc}[49m";

            public static AnsiControlCode Black => $"{Esc}[40m";

            public static AnsiControlCode Red { get; } = $"{Esc}[41m";
            public static AnsiControlCode Green { get; } = $"{Esc}[42m";
            public static AnsiControlCode Yellow { get; } = $"{Esc}[43m";
            public static AnsiControlCode Blue { get; } = $"{Esc}[44m";
            public static AnsiControlCode Magenta { get; } = $"{Esc}[45m";
            public static AnsiControlCode Cyan { get; } = $"{Esc}[46m";
            public static AnsiControlCode White { get; } = $"{Esc}[47m";
            public static AnsiControlCode DarkGray { get; } = $"{Esc}[100m";
            public static AnsiControlCode LightRed { get; } = $"{Esc}[101m";
            public static AnsiControlCode LightGreen { get; } = $"{Esc}[102m";
            public static AnsiControlCode LightYellow { get; } = $"{Esc}[103m";
            public static AnsiControlCode LightBlue { get; } = $"{Esc}[104m";
            public static AnsiControlCode LightMagenta { get; } = $"{Esc}[105m";
            public static AnsiControlCode LightCyan { get; } = $"{Esc}[106m";
            public static AnsiControlCode LightGray { get; } = $"{Esc}[107m";

            public static AnsiControlCode Rgb(byte r, byte g, byte b) => $"{Esc}[48;2;{r.ToString()};{g.ToString()};{b.ToString()}m";
        }

        [DebuggerStepThrough]
        public static class Foreground
        {
            public static AnsiControlCode Default => $"{Esc}[39m";

            public static AnsiControlCode Black { get; } = $"{Esc}[30m";
            public static AnsiControlCode Red { get; } = $"{Esc}[31m";
            public static AnsiControlCode Green { get; } = $"{Esc}[32m";
            public static AnsiControlCode Yellow { get; } = $"{Esc}[33m";
            public static AnsiControlCode Blue { get; } = $"{Esc}[34m";
            public static AnsiControlCode Magenta { get; } = $"{Esc}[35m";
            public static AnsiControlCode Cyan { get; } = $"{Esc}[36m";
            public static AnsiControlCode White { get; } = $"{Esc}[37m";
            public static AnsiControlCode DarkGray { get; } = $"{Esc}[90m";
            public static AnsiControlCode LightRed { get; } = $"{Esc}[91m";
            public static AnsiControlCode LightGreen { get; } = $"{Esc}[92m";
            public static AnsiControlCode LightYellow { get; } = $"{Esc}[93m";
            public static AnsiControlCode LightBlue { get; } = $"{Esc}[94m";
            public static AnsiControlCode LightMagenta { get; } = $"{Esc}[95m";
            public static AnsiControlCode LightCyan { get; } = $"{Esc}[96m";
            public static AnsiControlCode LightGray { get; } = $"{Esc}[97m";

            public static AnsiControlCode Rgb(byte r, byte g, byte b) => $"{Esc}[38;2;{r.ToString()};{g.ToString()};{b.ToString()}m";
        }
    }
}
