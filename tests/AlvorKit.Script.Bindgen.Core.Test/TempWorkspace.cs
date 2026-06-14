namespace AlvorKit.Script.Bindgen.Core.Test;

internal sealed class TempWorkspace : IDisposable
{
    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    public string Root { get; }

    public static TempWorkspace Create() =>
        new(Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Bindgen.Core.Test", Guid.NewGuid().ToString("N")));

    public string CreateDirectory(string name)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
