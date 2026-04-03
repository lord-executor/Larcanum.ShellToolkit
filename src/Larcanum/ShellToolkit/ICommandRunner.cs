namespace Larcanum.ShellToolkit;

public interface ICommandRunner
{
    IBoundCommand Bind(ICommand command);
    IBoundCommand Bind(IPipeline pipeline);
    Task<int> ExecAsync(ICommand cmd, CancellationToken ct = default);
    Task<int> ExecAsync(IPipeline pipeline, CancellationToken ct = default);
    Task<CommandResult> CaptureAsync(ICommand cmd, CancellationToken ct = default);
    Task<CommandResult> CaptureAsync(IPipeline pipeline, CancellationToken ct = default);
}
