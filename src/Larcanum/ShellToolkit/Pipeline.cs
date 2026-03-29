namespace Larcanum.ShellToolkit;

public class Pipeline : IPipeline
{
    private readonly List<IPipelineStep> _steps = new List<IPipelineStep>();

    public Pipeline(ICommand cmd)
    {
        _steps.Add(new CommandPipelineStep(cmd));
    }

    public IPipeline Pipe(ICommand cmd)
    {
        _steps.Add(new CommandPipelineStep(cmd));
        return this;
    }

    public IPipeline Pipe(FileInfo file)
    {
        _steps.Add(new FileSinkPipelineStep(file));
        return this;
    }

    public async Task<CommandResult> Run(PipelineOutput initial, OutputMode mode, CancellationToken ct = default)
    {
        // This process is certainly not the most efficient, especially for longer running commands with large
        // outputs. This approach waits for each step to complete before starting the next one and at any time there
        // are at most two child processes running (one producing output and one consuming output). At some point
        // it might become too inefficient, and we'll have to establish a proper _streaming_ pipeline which hopefully
        // should be possible without refactoring all the client code.
        var previous = initial;
        var lastExitCode = 0;
        for (var i = 0; i < _steps.Count; i++)
        {
            var isLast = (i == _steps.Count - 1);
            var output = await _steps[i].Connect(previous, isLast ? mode : OutputMode.Capture, ct);
            if (previous.Process != null)
            {
                await previous.Process.WaitForExitAsync(ct);
                lastExitCode = previous.Process.ExitCode;
                previous.Process.Dispose();
            }
            previous = output;
        }

        if (previous.Process != null)
        {
            await previous.Process.WaitForExitAsync(ct);
            lastExitCode = previous.Process.ExitCode;
            previous.Process.Dispose();

            return new CommandResult
            {
                ExitCode = lastExitCode,
                Output = previous.Out == null ? string.Empty : await previous.Out.ReadToEndAsync(ct)
            };
        }

        return new CommandResult { ExitCode = lastExitCode };
    }

    public override string ToString()
    {
        return string.Join(string.Empty, _steps).TrimStart(' ', '|');
    }
}
