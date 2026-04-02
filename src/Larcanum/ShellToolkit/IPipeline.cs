namespace Larcanum.ShellToolkit;

public interface IPipeline
{
    Task<CommandResult> Run(OutputMode mode = OutputMode.Default, CancellationToken ct = default)
    {
        return Run(EmptyPipelineOutput.Instance, mode, ct);
    }

    Task<CommandResult> Run(IPipelineOutput initial, OutputMode mode, CancellationToken ct = default);

    IPipeline Pipe(ICommand cmd);
    IPipeline Pipe(FileInfo file);
}
