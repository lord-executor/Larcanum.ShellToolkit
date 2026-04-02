using System.Diagnostics;

namespace Larcanum.ShellToolkit;

public class ProcessPipelineOutput : IPipelineOutput
{
    public Process? Process { get; }
    public StreamReader? Out { get; }

    public ProcessPipelineOutput(Process process)
    {
        Process = process;
        if (process.StartInfo.RedirectStandardOutput)
        {
            Out = process.StandardOutput;
        }
    }

    public async Task<int> WaitForExit(CancellationToken ct = default)
    {
        if (Process == null)
        {
            return 0;
        }

        await Process.WaitForExitAsync(ct);
        var exitCode = Process.ExitCode;
        Process.Dispose();

        return exitCode;
    }
}
