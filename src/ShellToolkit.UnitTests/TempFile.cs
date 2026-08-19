namespace ShellToolkit.UnitTests;

public class TempFile : IDisposable
{
    public FileInfo File { get; }

    public TempFile()
    {
        var path = Path.GetTempFileName();
        File = new FileInfo(path);
    }

    public void Dispose()
    {
        if (File.Exists)
        {
            File.Delete();
        }
    }
}
