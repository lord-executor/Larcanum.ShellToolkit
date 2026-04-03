namespace Larcanum.ShellToolkit;

public interface ICommandRunner
{
    IBoundCommand Bind(IPipeline pipeline);
}

public static class CommandRunnerExtensions
{
    extension(ICommandRunner runner)
    {
        public IBoundCommand Bind(ICommand command)
        {
            return runner.Bind(new Pipeline(new ProcessPipelineStep(command)));
        }

        public Task<int> ExecAsync(ICommand cmd, CancellationToken ct = default)
        {
            return runner.Bind(cmd).ExecAsync(ct);
        }

        public Task<int> ExecAsync(IPipeline pipeline, CancellationToken ct = default)
        {
            return runner.Bind(pipeline).ExecAsync(ct);
        }

        public Task<CommandResult> CaptureAsync(ICommand cmd, CancellationToken ct = default)
        {
            return runner.Bind(cmd).CaptureAsync(ct);
        }

        public Task<CommandResult> CaptureAsync(IPipeline pipeline, CancellationToken ct = default)
        {
            return runner.Bind(pipeline).CaptureAsync(ct);
        }
    }
}
