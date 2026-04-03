using System.Diagnostics;

namespace Larcanum.ShellToolkit;

public static class CommandExtensions
{
    extension(ICommand command)
    {
        public IPipeline Pipe(ICommand next)
        {
            return new Pipeline(command).Pipe(next);
        }

        public IPipeline Pipe(FileInfo file)
        {
            return new Pipeline(command).Pipe(file);
        }

        internal ProcessStartInfo ToProcessStartInfo()
        {
            var info = new ProcessStartInfo
            {
                FileName = command.CommandPath,
                WorkingDirectory = Environment.CurrentDirectory,
                // false is the default for .NET Core, but we want to be explicit here.
                UseShellExecute = false,
                // This is important for "forwarding" access to the console to the child process, and even though
                // false is the default value, we want to be explicit about this. This allows tools with fancy console
                // UI like menus and progress bars to work when created as child processes.
                CreateNoWindow = false,
            };

            foreach (var arg in command.Arguments)
            {
                info.ArgumentList.Add(arg.Argument);
            }

            return info;
        }
    }
}
