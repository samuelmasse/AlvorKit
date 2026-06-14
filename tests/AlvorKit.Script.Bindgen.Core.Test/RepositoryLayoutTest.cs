using AlvorKit.Script.Bindgen;

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

    /// <summary>Explicit library names are returned directly without scanning native directories.</summary>
    [TestMethod]
    public void SelectedLibraries_ReturnsExplicitSelection()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(new[] { "glfw" }, layout.SelectedLibraries("glfw").ToArray());
    }

    /// <summary>The all selection returns only bindgen-enabled native libraries in sorted order.</summary>
    [TestMethod]
    public void SelectedLibraries_ReturnsSortedBindgenLibraries()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native");
        Directory.CreateDirectory(Path.Combine(native, "zeta"));
        Directory.CreateDirectory(Path.Combine(native, "alpha"));
        Directory.CreateDirectory(Path.Combine(native, "ignored"));
        File.WriteAllText(Path.Combine(native, "zeta", "bindgen.json"), "{}");
        File.WriteAllText(Path.Combine(native, "alpha", "bindgen.json"), "{}");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, layout.SelectedLibraries("all").ToArray());
    }
}
