namespace Larcanum.ShellToolkit;

public interface IPipeline
{
    IPipeline Pipe(ICommand command);
    IPipeline Pipe(IPipelineStep step);
    Task<CommandResult> Run(IExecutionContext context, OutputMode mode, CancellationToken ct = default);
}

public static class PipelineExtensions
{
    extension(IPipeline pipeline)
    {
        public IPipeline Pipe(FileInfo file)
        {
            return pipeline.Pipe(new FileSinkPipelineStep(file));
        }
    }
}
