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

    /// <summary>Repository discovery reports a clear failure when no solution marker is present.</summary>
    [TestMethod]
    public void FindFrom_RejectsMissingSolutionRoot()
    {
        using var workspace = TempWorkspace.Create();

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => RepositoryLayout.FindFrom(Path.Combine(workspace.Root, "missing")));

        StringAssert.Contains(exception.Message, "AlvorKit.slnx not found above");
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
        File.WriteAllText(Path.Combine(conf, "bindgen.yml"), "{}");
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
        File.WriteAllText(Path.Combine(conf, "bindgen.yml"), "{}");
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
        File.WriteAllText(Path.Combine(native, "zeta", "conf", "bindgen.yml"), "{}");
        File.WriteAllText(Path.Combine(native, "alpha", "conf", "bindgen.yml"), "{}");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, layout.SelectedLibraries("all").ToArray());
    }

    /// <summary>The all selection is empty when the repository has no native directory.</summary>
    [TestMethod]
    public void SelectedLibraries_ReturnsEmptyWhenNativeDirectoryIsMissing()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        CollectionAssert.AreEqual(Array.Empty<string>(), layout.SelectedLibraries("all").ToArray());
    }

    /// <summary>Missing generated-output roots keep the configured bindgen project paths.</summary>
    [TestMethod]
    public void ResolveGeneratedOutputRoot_AllowsMissingOverride()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        Assert.IsNull(layout.ResolveGeneratedOutputRoot(null));
        Assert.IsNull(layout.ResolveGeneratedOutputRoot(""));
    }

    /// <summary>Relative and absolute generated-output roots are normalized when they stay under out.</summary>
    [TestMethod]
    public void ResolveGeneratedOutputRoot_AllowsOutDirectory()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var layout = RepositoryLayout.FindFrom(workspace.Root);
        var expected = Path.Combine(workspace.Root, "out", "bindgen-review", "after");

        Assert.AreEqual(Path.Combine(workspace.Root, "out"), layout.ResolveGeneratedOutputRoot("out"));
        Assert.AreEqual(expected, layout.ResolveGeneratedOutputRoot("out/bindgen-review/after"));
        Assert.AreEqual(expected, layout.ResolveGeneratedOutputRoot(expected));
    }

    /// <summary>Generated-output roots outside out are rejected so review snapshots cannot overwrite source files.</summary>
    [TestMethod]
    public void ResolveGeneratedOutputRoot_RejectsDirectoryOutsideOut()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var layout = RepositoryLayout.FindFrom(workspace.Root);

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => layout.ResolveGeneratedOutputRoot("generated/bindings"));

        StringAssert.Contains(exception.Message, "inside the repository out directory");
    }
}
