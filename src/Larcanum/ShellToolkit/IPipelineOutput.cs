namespace Larcanum.ShellToolkit;

public interface IPipelineOutput
{
    Stream Out { get; }
    Stream Error { get; }
    Task<int> WaitForExit(CancellationToken ct = default);
}

public static class PipelineOutputExtensinos
{
    extension(IPipelineOutput output)
    {
        public TextReader OutReader => new StreamReader(output.Out);
        public TextReader ErrorReader => new StreamReader(output.Error);

        public Task<string> ReadAllOutput(CancellationToken ct = default)
        {
            return output.OutReader.ReadToEndAsync(ct);
        }

        public Task<string> ReadAllError(CancellationToken ct = default)
        {
            return output.ErrorReader.ReadToEndAsync(ct);
        }

        public async Task<CommandResult> ToCommandResult(int exitCode, CancellationToken ct = default)
        {
            return new CommandResult
            {
                ExitCode = exitCode,
                Output = await output.ReadAllOutput(ct),
                Error = await output.ReadAllError(ct),
            };
        }
    }
}
