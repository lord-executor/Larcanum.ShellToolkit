using System.Diagnostics;

namespace Larcanum.ShellToolkit;

/// <summary>
/// A pipeline step that wraps a command so that it takes its input from the caller of the <see cref="Connect"/>
/// method and returns its <see cref="ProcessPipelineOutput"/> so that it can be used in the next step of the pipeline. This
/// is analogous to the shell "pipe" operator "|".
/// </summary>
public class ProcessPipelineStep : IPipelineStep
{
    private readonly ICommand _cmd;

    public ProcessPipelineStep(ICommand cmd)
    {
        _cmd = cmd;
    }

    public async Task<IPipelineOutput> Connect(IPipelineOutput? previous, OutputMode mode, CancellationToken ct = default)
    {
        var process = new Process { StartInfo = _cmd.ToProcessStartInfo() };

        if (mode == OutputMode.Capture)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
        }

        if (previous != null)
        {
            process.StartInfo.RedirectStandardInput = true;
            _ = previous.Out.CopyToAsync(process.StandardInput.BaseStream, ct)
                .ContinueWith(_ => process.StandardInput.Close(), ct);
        }

        process.Start();
        var output = new ProcessPipelineOutput(process);

        return output;
    }
}
