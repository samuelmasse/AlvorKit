namespace AlvorKit;

[TestClass]
public class RootFilesTest
{
    /// <summary>Root services resolve without Engine and child services can directly inject their RootFiles parent.</summary>
    [TestMethod]
    public void Injection_ResolvesRootServicesAndScopedParents()
    {
        var fs = new TestFs();
        fs.Add("host/shared.txt", "host");
        fs.Add("app/local.txt", "app");
        var injector = new Injector();
        injector.Add(new RootFileRoots(["host"]));
        var root = injector.Scope<RootScope>();
        root.Add<RootFs>(fs);
        var app = root.Scope<ResourceScope>();
        var sibling = root.Scope<ResourceScope>();
        var files = app.Get<ResourceFiles>();
        var services = app.Get<ResourceServices>();

        Assert.AreSame(fs, services.Fs);
        Assert.AreSame(root.Get<RootFiles>(), services.Files);
        Assert.AreSame(files, app.Get<ResourceFiles>());
        Assert.AreNotSame(files, sibling.Get<ResourceFiles>());
        Assert.AreEqual("host", files.ReadAllText("shared.txt"));
        Assert.AreEqual("app", files.ReadAllText("local.txt"));
    }

    /// <summary>The default filesystem is injected automatically and reads physical resources without registration.</summary>
    [TestMethod]
    public void Injection_UsesPhysicalFsWithoutRegistration()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("text.txt", "Mushroom 🍄");
        var injector = new Injector();
        injector.Add(new RootFileRoots([workspace.Root]));
        var root = injector.Scope<RootScope>();
        var files = root.Get<RootFiles>();

        Assert.AreEqual("Mushroom 🍄", files.ReadAllText("text.txt"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("Mushroom 🍄"), files.ReadAllBytes("text.txt"));

        using var reader = new StreamReader(files.OpenRead("text.txt"));
        Assert.AreEqual("Mushroom 🍄", reader.ReadToEnd());
    }

    /// <summary>Later roots override earlier roots, and child roots override parents without changing the parent view.</summary>
    [TestMethod]
    public void Lookup_UsesCopiedRootsAndOrderedParents()
    {
        var fs = new TestFs();
        fs.Add("host/shared.txt", "host");
        fs.Add("host/host.txt", "host only");
        fs.Add("app/shared.txt", "app");
        fs.Add("first/shared.txt", "first");
        fs.Add("last/shared.txt", "last");
        var root = new RootFiles(fs, new RootFileRoots(["host"]));
        var app = new ChildFiles(root, ["app"]);
        string[] roots = ["first", "last"];
        var module = new ChildFiles(app, roots);
        roots[1] = "missing";

        Assert.AreEqual("last", module.ReadAllText("shared.txt"));
        Assert.AreEqual("app", app.ReadAllText("shared.txt"));
        Assert.AreEqual("host", root.ReadAllText("shared.txt"));
        Assert.AreEqual("host only", module.ReadAllText("host.txt"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("last"), module.ReadAllBytes("shared.txt"));

        using var stream = module.OpenRead("shared.txt");
        Assert.AreEqual((int)'l', stream.ReadByte());
    }

    /// <summary>Repeated child reads cache even inherited resolutions while opening file contents afresh.</summary>
    [TestMethod]
    public void Lookup_CachesSuccessfulParentResolutions()
    {
        var fs = new TestFs();
        fs.Add("host/shared.txt", "host");
        var root = new RootFiles(fs, new RootFileRoots(["host"]));
        var child = new ChildFiles(root, ["app"]);

        Assert.AreEqual("host", child.ReadAllText("shared.txt"));
        var probes = fs.Probes;
        Assert.AreEqual("host", child.ReadAllText("shared.txt"));
        Assert.AreEqual(probes, fs.Probes);
        Assert.AreEqual(2, fs.Reads);
    }

    /// <summary>Existence checks use the same precedence and cache as reads without opening a file.</summary>
    [TestMethod]
    public void Exists_UsesResourceRootsAndParentsWithoutReading()
    {
        var fs = new TestFs();
        fs.Add("host/shared.txt", "host");
        fs.Add("host/parent.txt", "parent");
        fs.Add("app/shared.txt", "app");
        var root = new RootFiles(fs, new RootFileRoots(["host"]));
        var child = new ChildFiles(root, ["app"]);

        Assert.IsTrue(child.Exists("shared.txt"));
        Assert.IsTrue(child.Exists("parent.txt"));
        Assert.IsFalse(child.Exists("missing.txt"));
        Assert.AreEqual(0, fs.Reads);
        var probes = fs.Probes;

        Assert.AreEqual("app", child.ReadAllText("shared.txt"));
        Assert.AreEqual("parent", child.ReadAllText("parent.txt"));
        Assert.AreEqual(probes, fs.Probes);
    }

    /// <summary>A resource missing from the entire parent chain returns false from Exists and throws on a read.</summary>
    [TestMethod]
    public void MissingResource_ReportsLogicalName()
    {
        var root = new RootFiles(new TestFs(), new RootFileRoots(["host"]));
        var child = new ChildFiles(root, ["app"]);

        Assert.IsFalse(child.Exists("missing.txt"));
        var exception = Assert.ThrowsException<FileNotFoundException>(() => child.ReadAllText("missing.txt"));

        Assert.AreEqual("missing.txt", exception.FileName);
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    private class ResourceAttribute : InjectorAttribute;

    [Resource]
    private class ResourceScope : InjectorScope<ResourceAttribute>;

    [Resource]
    private class ResourceFiles(RootFiles parent) : RootFiles(parent, ["app"]);

    [Resource]
    private record ResourceServices(RootFs Fs, RootFiles Files);

    private class ChildFiles(RootFiles parent, ReadOnlySpan<string> roots) : RootFiles(parent, roots);

    [Root]
    private class TestFs : RootFs
    {
        private readonly Dictionary<string, string> files = [];
        private int probes;
        private int reads;

        public int Probes => probes;
        public int Reads => reads;

        public void Add(string path, string contents) => files.Add(path, contents);

        public override bool Exists(string path)
        {
            probes++;
            return files.ContainsKey(path.Replace('\\', '/'));
        }

        public override string ReadAllText(string path)
        {
            reads++;
            return files[path.Replace('\\', '/')];
        }

        public override byte[] ReadAllBytes(string path) => Encoding.UTF8.GetBytes(ReadAllText(path));

        public override Stream OpenRead(string path) => new MemoryStream(ReadAllBytes(path));
    }
}
