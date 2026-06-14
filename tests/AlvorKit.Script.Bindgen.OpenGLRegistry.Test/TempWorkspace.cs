namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

internal sealed class TempWorkspace : IDisposable
{
    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    public string Root { get; }

    public static TempWorkspace Create() =>
        new(Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Bindgen.OpenGLRegistry.Test", Guid.NewGuid().ToString("N")));

    public string WriteFile(string name, string contents)
    {
        var path = Path.Combine(Root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
