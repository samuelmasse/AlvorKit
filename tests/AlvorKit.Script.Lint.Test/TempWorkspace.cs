namespace AlvorKit.Script.Lint.Test;

/// <summary>Temporary filesystem workspace used by lint planner tests.</summary>
internal sealed class TempWorkspace : IDisposable
{
    /// <summary>Creates a temporary workspace under the system temp directory.</summary>
    private TempWorkspace(string root) =>
        Root = root;

    /// <summary>Absolute workspace root path.</summary>
    public string Root { get; }

    /// <summary>Creates an empty temporary workspace.</summary>
    public static TempWorkspace Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Lint.Test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new(root);
    }

    /// <summary>Writes a text file inside the workspace, creating parent directories as needed.</summary>
    public void Write(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Root);
        File.WriteAllText(path, content);
    }

    /// <summary>Deletes the temporary workspace.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
