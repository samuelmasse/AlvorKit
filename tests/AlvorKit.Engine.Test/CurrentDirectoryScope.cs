namespace AlvorKit.Engine.Test;

/// <summary>Temporarily changes the process current directory for asset-resolution tests.</summary>
internal sealed class CurrentDirectoryScope : IDisposable
{
    private readonly string previous = Environment.CurrentDirectory;

    /// <summary>Changes the current directory until the scope is disposed.</summary>
    internal CurrentDirectoryScope(string directory) => Environment.CurrentDirectory = directory;

    /// <summary>Restores the directory that was active when the scope was created.</summary>
    public void Dispose() => Environment.CurrentDirectory = previous;
}
