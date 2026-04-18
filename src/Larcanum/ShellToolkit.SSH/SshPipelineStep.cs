using Renci.SshNet;

namespace Larcanum.ShellToolkit.SSH;

public class SshPipelineStep : IPipelineStep
{
    private readonly ISshClient _client;
    private readonly ICommand _cmd;

    public SshPipelineStep(ISshClient client, ICommand cmd)
    {
        _client = client;
        _cmd = cmd;
    }

    public async Task<IPipelineOutput> Connect(IPipelineOutput? previous, OutputMode mode, CancellationToken ct = default)
    {
        var cmd = _client.CreateCommand(_cmd.ToCommandText());

        if (mode != OutputMode.Capture)
        {
            _ = cmd.OutputStream.CopyToAsync(Console.OpenStandardOutput(), ct);
            _ = cmd.ExtendedOutputStream.CopyToAsync(Console.OpenStandardError(), ct);
        }

        var task = cmd.ExecuteAsync(ct).ContinueWith(_ =>
        {
            var exitCode = cmd.ExitStatus ?? -1;
            cmd.Dispose();
            return exitCode;
        }, ct);

        // The input stream is only available during execution of the command, so we have to start it first and then
        // copy the output from the previous step into the input stream.
        if (previous != null)
        {
            var stdin = cmd.CreateInputStream();
            _ = previous.Out.CopyToAsync(stdin, ct)
                .ContinueWith(_ => stdin.Dispose(), ct);
        }

        if (mode == OutputMode.Capture)
        {
            return new StreamPipelineOutput(cmd.OutputStream, cmd.ExtendedOutputStream, task);
        }

        return new StreamPipelineOutput(Stream.Null, Stream.Null, task);
    }
}
