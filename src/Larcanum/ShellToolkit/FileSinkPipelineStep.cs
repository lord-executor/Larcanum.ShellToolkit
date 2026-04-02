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

    public async Task<IPipelineOutput> Connect(IPipelineOutput previous, OutputMode mode, CancellationToken ct = default)
    {
        if (previous.Out == null)
        {
            throw new InvalidOperationException("Previous pipeline step did not provide a connectable output");
        }

        if (mode == OutputMode.Capture)
        {
            throw new InvalidOperationException("Cannot capture output from a file sink");
        }

        await using var stream = _file.Open(FileMode.OpenOrCreate);
        // truncate
        stream.SetLength(0);
        await previous.Out.BaseStream.CopyToAsync(stream, ct);

        return EmptyPipelineOutput.Instance;
    }

    public override string ToString()
    {
        return $" > {_file.FullName}";
    }
}
