namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Creates disposable directories for bindgen core unit tests.</summary>
internal sealed class TempWorkspace : IDisposable
{
    /// <summary>Creates the workspace root.</summary>
    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    public string Root { get; }

    /// <summary>Creates a new isolated temp workspace.</summary>
    public static TempWorkspace Create() =>
        new(Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Bindgen.Core.Test", Guid.NewGuid().ToString("N")));

    /// <summary>Creates a child directory and returns its absolute path.</summary>
    public string CreateDirectory(string name)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Deletes the workspace directory when the test ends.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
