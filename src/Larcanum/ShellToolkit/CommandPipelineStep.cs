using System.Diagnostics;

namespace Larcanum.ShellToolkit;

/// <summary>
/// A pipeline step that wraps a command so that it takes its input from the caller of the <see cref="Connect"/>
/// method and returns its <see cref="ProcessPipelineOutput"/> so that it can be used in the next step of the pipeline. This
/// is analogous to the shell "pipe" operator "|".
/// </summary>
public class CommandPipelineStep : IPipelineStep
{
    private readonly ICommand _cmd;

    public CommandPipelineStep(ICommand cmd)
    {
        _cmd = cmd;
    }

    public async Task<IPipelineOutput> Connect(IPipelineOutput previous, OutputMode mode, CancellationToken ct = default)
    {
        var process = new Process { StartInfo = _cmd.ToProcessStartInfo() };

        if (mode == OutputMode.Capture)
        {
            process.StartInfo.RedirectStandardOutput = true;
        }

        if (previous.Out != null)
        {
            process.StartInfo.RedirectStandardInput = true;
        }

        process.Start();

        if (previous.Out != null)
        {
            await previous.Out.BaseStream.CopyToAsync(process.StandardInput.BaseStream, ct);
            // If the STDIN of the process is not closed, then it can never complete, so we want to
            // close it as soon as we have copied all the data from the previous pipeline step.
            process.StandardInput.Close();
        }

        return new ProcessPipelineOutput(process);
    }

    public override string ToString()
    {
        return $" | {_cmd}";
    }
}
