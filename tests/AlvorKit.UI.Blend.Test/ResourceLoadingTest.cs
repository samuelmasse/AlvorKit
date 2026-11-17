namespace AlvorKit;

[TestClass]
public class ResourceLoadingTest
{
    /// <summary>Resource fonts use filesystem bytes, cache by path, and release native fonts before their GL layer.</summary>
    [TestMethod]
    public void Fonts_ReadVirtualFilesAndClearCacheOnUnload()
    {
        using var h = new BlendTestHarness();
        using var gl = new BlendTestGlLayer(h.AppGl);
        var fs = new ResourceFs();
        fs.Add("virtual/Test.ttf", FontData());
        var files = new RootFiles(fs, ["virtual"]);
        var ft = new FontOwnershipFt();
        var fonts = new ScopedFonts(h.Root.Get<RootFontPacking>(), ft, files, gl);
        var first = fonts["Test.ttf"];

        Assert.AreSame(first, fonts["Test.ttf"]);
        Assert.AreEqual(1, fs.ByteReads);
        Assert.IsTrue(first.Size(16).GlyphSlot(new Rune('A')).Glyph.Box.Y > 0);
        var explicitFont = fonts.Open(new() { File = "Test.ttf" });
        Assert.AreNotSame(first, explicitFont);
        Assert.AreEqual(2, fs.ByteReads);
        var texture = first.Textures[0].Id;

        fonts.Unload();
        Assert.AreEqual(2, ft.FaceCloses);
        Assert.AreEqual(1, ft.LibraryCloses);
        Assert.AreEqual(2, ft.FacesClosedBeforeLibrary);
        Assert.IsTrue(h.Backend.HasTexture(texture));

        var reopened = fonts["Test.ttf"];
        Assert.AreNotSame(first, reopened);
        Assert.AreEqual(3, fs.ByteReads);
        fonts.Unload();
        gl.Dispose();
        Assert.IsFalse(h.Backend.HasTexture(texture));
        h.Draw();
    }

    /// <summary>Unused resource font services stay lazy even when unload is their first operation.</summary>
    [TestMethod]
    public void Fonts_UnusedOwnerCreatesNoNativeContext()
    {
        using var h = new BlendTestHarness();
        var ft = new FontOwnershipFt();
        var fonts = new RootFonts(h.Root.Get<RootFontPacking>(), ft, h.Root.Get<RootFiles>(), h.Root.Get<RootGl>());

        fonts.Unload();
        fonts.Unload();

        Assert.AreEqual(0, ft.LibraryOpens);
        Assert.AreEqual(0, ft.LibraryCloses);
        Assert.AreEqual(0, ft.FaceCloses);
    }

    /// <summary>Each texture owner resolves its own files, closes streams, and keeps its GPU cache until its scope ends.</summary>
    [TestMethod]
    public void Textures_UseScopedFilesCachesAndGlOwnership()
    {
        using var h = new BlendTestHarness();
        using var gl = new BlendTestGlLayer(h.AppGl);
        var fs = new ResourceFs();
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACAgMAAAAP2OW3AAAADFBMVEUAAAAAAAD/AAAAAP/8L3PX" +
            "AAAABHRSTlMA/4D/G4olsQAAAAxJREFUeJxjEGDYAAAA5ADBJ6joVwAAAABJRU5ErkJggg==");
        fs.Add("host/Image.png", png);
        fs.Add("module/Image.png", png);
        var files = new RootFiles(fs, ["host"]);
        var rootTextures = new RootTextures(new RootPngs(), files, h.Root.Get<RootGl>());
        var textures = new ScopedTextures(new RootPngs(), new ScopedFiles(files, ["module"]), gl);
        var rootTexture = rootTextures["Image.png"];
        var texture = textures["Image.png"];

        Assert.AreSame(texture, textures["Image.png"]);
        Assert.AreNotSame(rootTexture, texture);
        Assert.AreEqual(new Vec2u(2, 2), texture.Size);
        Assert.AreEqual(2, fs.StreamOpens);
        Assert.AreEqual("module/Image.png", fs.LastPath);
        Assert.IsFalse(fs.LastStream.CanRead);
        Assert.IsTrue(h.Backend.HasTexture(texture.Id));

        gl.Dispose();

        Assert.IsFalse(h.Backend.HasTexture(texture.Id));
        Assert.IsTrue(h.Backend.HasTexture(rootTexture.Id));
        h.Draw();
    }

    private byte[] FontData()
    {
        using var stream = typeof(RootRoboto).Assembly.GetManifestResourceStream("AlvorKit.Engine.res.fonts.RobotoMono-Regular.ttf")!;
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private class ScopedFonts(RootFontPacking packing, Ft ft, RootFiles files, GlLayer gl) : RootFonts(packing, ft, files, gl);

    private class ScopedTextures(RootPngs pngs, RootFiles files, GlLayer gl) : RootTextures(pngs, files, gl);

    private class ScopedFiles(RootFiles parent, ReadOnlySpan<string> roots) : RootFiles(parent, roots);

    private class ResourceFs : RootFs
    {
        private readonly Dictionary<string, byte[]> files = [];
        private int byteReads;
        private int streamOpens;
        private string lastPath = "";
        private MemoryStream? lastStream;

        public int ByteReads => byteReads;
        public int StreamOpens => streamOpens;
        public string LastPath => lastPath;
        public Stream LastStream => lastStream!;

        public void Add(string path, byte[] data) => files.Add(path, data);

        public override bool Exists(string path) => files.ContainsKey(path.Replace('\\', '/'));

        public override byte[] ReadAllBytes(string path)
        {
            byteReads++;
            return files[path.Replace('\\', '/')];
        }

        public override Stream OpenRead(string path)
        {
            streamOpens++;
            lastPath = path.Replace('\\', '/');
            lastStream = new MemoryStream(files[lastPath]);
            return lastStream;
        }
    }
}
