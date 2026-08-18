using System.Diagnostics;

namespace Larcanum.ShellToolkit;

public class ProcessPipelineOutput : IPipelineOutput
{
    public Process? Process { get; }
    public Stream Out { get; }
    public Stream Error { get; }

    public ProcessPipelineOutput(Process process)
    {
        Process = process;
        Out = new PipeStream();
        Error = new PipeStream();

        if (Process.StartInfo.RedirectStandardOutput)
        {
            Process.StandardOutput.BaseStream.CopyToAsync(Out);
        }
        if (Process.StartInfo.RedirectStandardError)
        {
            Process.StandardError.BaseStream.CopyToAsync(Error);
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
        await Out.DisposeAsync();
        await Error.DisposeAsync();
        Process.Dispose();

        return exitCode;
    }
}
