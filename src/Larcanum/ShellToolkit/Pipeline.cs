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

    public async Task<CommandResult> Run(IPipelineOutput? initial, OutputMode mode, CancellationToken ct = default)
    {
        // This process is starting each pipeline step in order and connecting its output stream to the next step.
        // Then we wait for each step to exit and read the final output. If any of the steps fail, we return the output
        // of the first failed step.
        var previous = initial;
        var outputChain = new List<IPipelineOutput>();

        if (previous != null)
        {
            outputChain.Add(previous);
        }

        foreach (var li in ListItems(_steps))
        {
            var output = await li.Item.Connect(previous, li.IsLast ? mode : OutputMode.Capture, ct);
            outputChain.Add(output);
            previous = output;
        }

        CommandResult? result = null;
        foreach (var li in ListItems(outputChain))
        {
            var exitCode = await li.Item.WaitForExit(ct);
            if (result == null && (exitCode != 0  || li.IsLast))
            {
                result = new CommandResult
                {
                    ExitCode = exitCode,
                    Output = await li.Item.ReadAllOutput(ct),
                    Error = await li.Item.ReadAllError(ct),
                };
            }
        }

        return result ?? throw new InvalidOperationException("Pipeline has no result. This should never happen.");
    }

    public override string ToString()
    {
        return string.Join(string.Empty, _steps).TrimStart(' ', '|');
    }

    private static IEnumerable<ListItem<T>> ListItems<T>(List<T> list)
    {
        return list.Select((item, index) => new ListItem<T>
        {
            Item = item,
            Index = index,
            IsFirst = index == 0,
            IsLast = index == list.Count - 1
        });
    }

    private class ListItem<T>
    {
        public required T Item { get; init; }
        public required int Index { get; init; }
        public required bool IsFirst { get; init; }
        public required bool IsLast { get; init; }
    }
}
