namespace Larcanum.ShellToolkit;

public interface IBoundCommand
{
    Task<CommandResult> CaptureAsync(CancellationToken ct = default);
    Task<int> ExecAsync(CancellationToken ct = default);

    IBoundCommand ThrowOnError();
}

public static class BoundCommandExtensions
{
    extension(IBoundCommand command)
    {
        public Task<string> CaptureAsStringAsync(CancellationToken ct = default) =>
            command.ThrowOnError().CaptureAsync(ct).ContinueWith(t => t.Result.AsString(), ct);

        public Task<string?> CaptureAsNullableStringAsync(CancellationToken ct = default) =>
            command.ThrowOnError().CaptureAsync(ct).ContinueWith(t => t.Result.AsNullableString(), ct);
    }
}
