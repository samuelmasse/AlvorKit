namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Temporary filesystem workspace for OpenGL registry tests.</summary>
internal sealed class TempWorkspace : IDisposable
{
    /// <summary>Creates a temporary workspace rooted at the supplied directory.</summary>
    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    /// <summary>Workspace root directory.</summary>
    public string Root { get; }

    /// <summary>Creates a new unique temporary workspace.</summary>
    public static TempWorkspace Create() =>
        new(Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Bindgen.OpenGLRegistry.Test", Guid.NewGuid().ToString("N")));

    /// <summary>Writes a file relative to the workspace root and returns its path.</summary>
    public string WriteFile(string name, string contents)
    {
        var path = Path.Combine(Root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    /// <summary>Deletes the temporary workspace.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
