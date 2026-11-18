namespace AlvorKit;

// Later roots take precedence, then the parent. Successful paths are cached;
// resource placement must remain stable for this instance's lifetime.
[Root]
public class RootFiles(RootFs fs, ReadOnlySpan<string> roots)
{
    private readonly RootFs fs = fs;
    private readonly string[] roots = [.. roots];
    private readonly RootFiles? parent;
    private readonly Dictionary<string, string> resolved = [];

    protected RootFiles(RootFiles parent, ReadOnlySpan<string> roots) : this(parent.fs, roots)
    {
        this.parent = parent;
    }

    public bool Exists(string path) => Find(path) != null;

    public string Resolve(string path) =>
        Find(path) ?? throw new FileNotFoundException($"Resource file not found: {path}", path);

    public Stream OpenRead(string path) => fs.OpenRead(Resolve(path));

    public string ReadAllText(string path) => fs.ReadAllText(Resolve(path));

    public byte[] ReadAllBytes(string path) => fs.ReadAllBytes(Resolve(path));

    private string? Find(string path)
    {
        if (resolved.TryGetValue(path, out var file))
            return file;

        file = FindInRoots(path) ?? parent?.Find(path);

        if (file != null)
            resolved.Add(path, file);

        return file;
    }

    private string? FindInRoots(string path)
    {
        for (var i = roots.Length - 1; i >= 0; i--)
        {
            var file = Path.Join(roots[i], path);

            if (fs.Exists(file))
                return file;
        }

        return null;
    }
}
