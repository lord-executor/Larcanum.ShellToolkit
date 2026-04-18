namespace Larcanum.ShellToolkit;

/// <summary>
/// Provides a pipeline sink that writes the output to a file. This works just like the shell redirect ">".
/// </summary>
public class FileSinkPipelineStep : IPipelineStep
{
    private readonly FileInfo _file;

    public FileSinkPipelineStep(FileInfo file)
    {
        _file = file;
    }

    public Task<IPipelineOutput> Connect(IPipelineOutput? previous, OutputMode mode, CancellationToken ct = default)
    {
        if (previous == null)
        {
            throw new InvalidOperationException("Previous pipeline step did not provide a connectable output");
        }

        if (mode == OutputMode.Capture)
        {
            throw new InvalidOperationException("Cannot capture output from a file sink");
        }

        var stream = _file.Open(FileMode.OpenOrCreate);
        // truncate
        stream.SetLength(0);
        var resultTask = previous.Out.CopyToAsync(stream, ct)
            .ContinueWith(_ =>
            {
                stream.DisposeAsync();
                return 0;
            }, ct);

        return Task.FromResult<IPipelineOutput>(new EmptyPipelineOutput(resultTask));
    }

    public override string ToString()
    {
        return $" > {_file.FullName}";
    }
}
