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
            };

            foreach (var arg in command.Arguments)
            {
                info.ArgumentList.Add(arg.Argument);
            }

            return info;
        }
    }
}
