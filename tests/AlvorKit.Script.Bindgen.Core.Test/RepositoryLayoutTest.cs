namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers repository discovery and native library selection.</summary>
[TestClass]
public sealed class RepositoryLayoutTest
{
    /// <summary>Repository discovery walks upward until it finds the solution marker.</summary>
    [TestMethod]
    public void FindFrom_WalksUpToSolutionRoot()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var nested = Path.Combine(workspace.Root, "a", "b", "c");
        Directory.CreateDirectory(nested);

        var layout = RepositoryLayout.FindFrom(nested);

        Assert.AreEqual(workspace.Root, layout.Root);
        Assert.AreEqual(Path.Combine(workspace.Root, "native"), layout.NativeDirectory);
    }

    /// <summary>Explicit library names are returned when they have bindgen metadata.</summary>
    [TestMethod]
    public void SelectedLibraries_ReturnsExplicitSelection()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native", "glfw");
        var conf = Path.Combine(native, "conf");
        Directory.CreateDirectory(conf);
        File.WriteAllText(Path.Combine(conf, "bindgen.json"), "{}");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(new[] { "glfw" }, layout.SelectedLibraries("GLFW").ToArray());
    }

    /// <summary>Unknown explicit library names fail with the discovered bindgen library list.</summary>
    [TestMethod]
    public void SelectedLibraries_RejectsUnknownSelection()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native", "alpha");
        var conf = Path.Combine(native, "conf");
        Directory.CreateDirectory(conf);
        File.WriteAllText(Path.Combine(conf, "bindgen.json"), "{}");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        var exception = Assert.ThrowsException<InvalidOperationException>(() => layout.SelectedLibraries("glfw").ToArray());

        StringAssert.Contains(exception.Message, "Unknown bindgen library 'glfw'");
        StringAssert.Contains(exception.Message, "alpha");
    }

    /// <summary>The all selection returns only bindgen-enabled native libraries in sorted order.</summary>
    [TestMethod]
    public void SelectedLibraries_ReturnsSortedBindgenLibraries()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native");
        Directory.CreateDirectory(Path.Combine(native, "zeta", "conf"));
        Directory.CreateDirectory(Path.Combine(native, "alpha", "conf"));
        Directory.CreateDirectory(Path.Combine(native, "ignored"));
        File.WriteAllText(Path.Combine(native, "zeta", "conf", "bindgen.json"), "{}");
        File.WriteAllText(Path.Combine(native, "alpha", "conf", "bindgen.json"), "{}");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, layout.SelectedLibraries("all").ToArray());
    }
}
