namespace Larcanum.ShellToolkit;

public class Pipeline : IPipeline
{
    private readonly IPipelineOutput? _initial;
    private readonly List<StepProxy> _steps = [];

    public Pipeline(IPipelineOutput? initial = null)
    {
        _initial = initial;
    }

    public IPipeline Pipe(ICommand command)
    {
        _steps.Add(new StepProxy(context => context.CreatePipelineStep(command), $" | {command}"));
        return this;
    }

    public IPipeline Pipe(IPipelineStep step)
    {
        _steps.Add(new StepProxy(_ => step, step.ToString()!));
        return this;
    }

    public async Task<CommandResult> Run(IExecutionContext context, OutputMode mode, CancellationToken ct = default)
    {
        // This process is starting each pipeline step in order and connecting its output stream to the next step.
        // Then we wait for each step to exit and read the final output. If any of the steps fail, we return the output
        // of the first failed step.
        var previous = _initial;
        var outputChain = new List<IPipelineOutput>();

        if (previous != null)
        {
            outputChain.Add(previous);
        }

        foreach (var li in ListItems(_steps.Select(p => p.StepFactory(context)).ToList()))
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
                result = await li.Item.ToCommandResult(exitCode, ct);
            }
        }

        return result ?? throw new InvalidOperationException("Pipeline has no result. This should never happen.");
    }

    public override string ToString()
    {
        return string.Join(string.Empty, _steps.Select(p => p.DisplayText)).TrimStart(' ', '|');
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

    delegate IPipelineStep PipelineStepFactory(IExecutionContext context);

    private record StepProxy(PipelineStepFactory StepFactory, string DisplayText);

    private class ListItem<T>
    {
        public required T Item { get; init; }
        public required int Index { get; init; }
        public required bool IsFirst { get; init; }
        public required bool IsLast { get; init; }
    }
}
