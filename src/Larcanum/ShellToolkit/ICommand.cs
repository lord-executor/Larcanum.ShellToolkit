using System.Diagnostics;

namespace Larcanum.ShellToolkit;

public interface ICommand
{
    string CommandPath { get; }
    IEnumerable<IArg> Arguments { get; }
    ProcessStartInfo ToProcessStartInfo();
    string ToCommandText() => Arguments.Any()
        ? $"{CommandPath} {string.Join(" ", Arguments.Select(a => a.Argument))}"
        : CommandPath;
}
