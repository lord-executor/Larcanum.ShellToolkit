namespace Larcanum.ShellToolkit;

public interface IPipeline
{
    IPipeline AddStep(IPipelineStep step);
    Task<CommandResult> Run(IPipelineOutput? initial, OutputMode mode, CancellationToken ct = default);
}

public static class PipelineExtensions
{
    extension(IPipeline pipeline)
    {
        public Task<CommandResult> Run(OutputMode mode = OutputMode.Default, CancellationToken ct = default)
        {
            return pipeline.Run(null, mode, ct);
        }

        public IPipeline Pipe(ICommand cmd)
        {
            return pipeline.AddStep(new ProcessPipelineStep(cmd));
        }

        public IPipeline Pipe(FileInfo file)
        {
            return pipeline.AddStep(new FileSinkPipelineStep(file));
        }
    }
}
