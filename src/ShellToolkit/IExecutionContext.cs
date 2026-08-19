using Microsoft.Extensions.Logging;

namespace Larcanum.ShellToolkit;

public interface IExecutionContext
{
    Settings Settings { get; }
    ILogger Logger { get; }

    void LogCommand(object command, CommandMode mode);
    IPipelineStep CreatePipelineStep(ICommand command);
}
