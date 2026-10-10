namespace AlvorKit;

/// <summary>Exercises scoped font ownership alongside Blend's root fonts and the real render-time packer.</summary>
[TestClass]
public class FontOwnershipTest
{
    /// <summary>Resolving and unloading an unused collection never creates a native font context.</summary>
    [TestMethod]
    public void UnusedCollection_UnloadDoesNotCreateNativeResources()
    {
        using var h = new BlendTestHarness();
        var ft = new FontOwnershipFt();
        var fonts = new FontCollection(h.Root.Get<RootFontPacking>(), ft, h.AppGl);

        fonts.Unload();
        fonts.Unload();

        Assert.AreEqual(0, ft.LibraryOpens);
        Assert.AreEqual(0, ft.LibraryCloses);
        Assert.AreEqual(0, ft.FaceCloses);
    }

    /// <summary>Unloading releases each face before its shared library, while atlas textures remain owned by the GL layer.</summary>
    [TestMethod]
    public void Unload_ReleasesNativeFontsAndLeavesGpuResourcesToLayer()
    {
        using var h = new BlendTestHarness();
        using var gl = new BlendTestGlLayer(h.AppGl);
        var ft = new FontOwnershipFt();
        var fonts = new FontCollection(h.Root.Get<RootFontPacking>(), ft, gl);
        var options = new FontOptions { Data = FontData() };
        var first = fonts.Open(options);
        var second = fonts.Open(options);
        var texture = first.Textures[0].Id;
        var rootTexture = h.Root.Get<RootInter>().Regular.Textures[0].Id;
        Assert.AreEqual(1, ft.LibraryOpens);
        Assert.AreNotSame(first, second);

        fonts.Unload();
        fonts.Unload();

        Assert.AreEqual(2, ft.FaceCloses);
        Assert.AreEqual(1, ft.LibraryCloses);
        Assert.AreEqual(2, ft.FacesClosedBeforeLibrary);
        Assert.IsTrue(h.Backend.HasTexture(texture));
        gl.Dispose();
        Assert.IsFalse(h.Backend.HasTexture(texture));
        Assert.IsTrue(h.Backend.HasTexture(rootTexture));
        h.Draw();
    }

    /// <summary>An unloaded collection can reopen with a fresh native context while its GL layer remains alive.</summary>
    [TestMethod]
    public void OpenAfterUnload_CreatesFreshOwnedFont()
    {
        using var h = new BlendTestHarness();
        var ft = new FontOwnershipFt();
        var fonts = new FontCollection(h.Root.Get<RootFontPacking>(), ft, h.AppGl);
        var options = new FontOptions { Data = FontData() };
        var old = fonts.Open(options);
        fonts.Unload();

        var current = fonts.Open(options);

        Assert.AreNotSame(old, current);
        Assert.AreEqual(2, ft.LibraryOpens);
        Assert.AreEqual(1, ft.LibraryCloses);
        Assert.AreEqual(1, ft.FaceCloses);
        Assert.IsTrue(current.Size(16).GlyphSlot(new Rune('A')).Glyph.Box.Y > 0);
        fonts.Unload();
        Assert.AreEqual(2, ft.LibraryCloses);
        Assert.AreEqual(2, ft.FaceCloses);
    }

    /// <summary>Packing skips unloaded collections and still repacks a surviving collection's full atlas.</summary>
    [TestMethod]
    public void Packing_AfterOwnerUnloadPreservesOtherCollections()
    {
        using var h = new BlendTestHarness();
        using var removedGl = new BlendTestGlLayer(h.AppGl);
        using var survivingGl = new BlendTestGlLayer(h.AppGl);
        var packing = h.Root.Get<RootFontPacking>();
        var removedFonts = new FontCollection(packing, new FtBackend(), removedGl);
        var survivingFonts = new FontCollection(packing, new FtBackend(), survivingGl);
        var options = new FontOptions { Data = FontData() };
        var removed = removedFonts.Open(options);
        var surviving = survivingFonts.Open(options);
        var removedSlot = FillAtlas(removed);
        var survivingSlot = FillAtlas(surviving);
        var previousTexture = survivingSlot.Texture;

        removedFonts.Unload();
        removedGl.Dispose();
        packing.Pack();

        Assert.IsFalse(h.Backend.HasTexture(removedSlot.Texture.Id));
        Assert.AreNotSame(previousTexture, survivingSlot.Texture);
        Assert.IsTrue(h.Backend.HasTexture(survivingSlot.Texture.Id));
        survivingFonts.Unload();
        survivingGl.Dispose();
        h.Draw();
    }

    /// <summary>Repeated packing of settled collections performs no managed allocation.</summary>
    [TestMethod]
    public void Packing_SettledCollectionsDoNotAllocate()
    {
        using var h = new BlendTestHarness();
        var packing = h.Root.Get<RootFontPacking>();

        for (var i = 0; i < 100; i++)
            packing.Pack();

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
            packing.Pack();

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    private byte[] FontData()
    {
        using var stream = typeof(RootRoboto).Assembly.GetManifestResourceStream("AlvorKit.Engine.res.fonts.RobotoMono-Regular.ttf")!;
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private FontGlyphSlot FillAtlas(Font font)
    {
        var size = font.Size(1800);
        var first = size.GlyphSlot(new Rune('W'));

        foreach (var character in "MBQ")
            size.GlyphSlot(new Rune(character));

        Assert.IsTrue(font.Textures.Length > 1);
        return first;
    }
}
