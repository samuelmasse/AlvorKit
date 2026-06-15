namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers generated output filesystem and MSBuild helpers.</summary>
[TestClass]
public sealed class GeneratedOutputTest
{
    /// <summary>Recreating a directory removes stale generated files and leaves the directory present.</summary>
    [TestMethod]
    public void RecreateDirectory_RemovesPreviousContentsAndLeavesDirectoryReady()
    {
        using var workspace = TempWorkspace.Create();
        var directory = workspace.CreateDirectory("generated");
        File.WriteAllText(Path.Combine(directory, "stale.txt"), "old");

        GeneratedOutput.RecreateDirectory(directory);

        Assert.IsTrue(Directory.Exists(directory));
        Assert.IsFalse(File.Exists(Path.Combine(directory, "stale.txt")));
    }

    /// <summary>Shared props include the generated project defaults required by consuming packages.</summary>
    [TestMethod]
    public void EmitSharedProps_CarriesGeneratedProjectDefaults()
    {
        var props = GeneratedOutput.EmitSharedProps();

        StringAssert.Contains(props, "<TargetFramework>net10.0</TargetFramework>");
        StringAssert.Contains(props, "<GeneratePackageOnBuild>false</GeneratePackageOnBuild>");
        StringAssert.Contains(props, "<GenerateDocumentationFile>true</GenerateDocumentationFile>");
        StringAssert.Contains(props, "<NoWarn>$(NoWarn);CS1573;CS1591</NoWarn>");
    }

    /// <summary>Default project layouts use the configured repository-relative project paths.</summary>
    [TestMethod]
    public void ResolveProjectLayout_UsesConfiguredPathsByDefault()
    {
        using var workspace = TempWorkspace.Create();

        var layout = GeneratedOutput.ResolveProjectLayout(
            workspace.Root,
            outputRoot: null,
            "out/bindgen/Fixture",
            "out/bindgen/Fixture.Backend");

        Assert.AreEqual(Path.Combine(workspace.Root, "out", "bindgen"), layout.Root);
        Assert.AreEqual(Path.Combine(workspace.Root, "out", "bindgen", "Fixture"), layout.ApiDirectory);
        Assert.AreEqual(Path.Combine(workspace.Root, "out", "bindgen", "Fixture.Backend"), layout.BackendDirectory);
    }

    /// <summary>Alternate output roots place generated projects directly under the requested root.</summary>
    [TestMethod]
    public void ResolveProjectLayout_ReplacesConfiguredBindingRoot()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = Path.Combine(workspace.Root, "out", "bindgen-review", "after");

        var layout = GeneratedOutput.ResolveProjectLayout(
            workspace.Root,
            outputRoot,
            "out/bindgen/Fixture",
            "out/bindgen/Fixture.Backend");

        Assert.AreEqual(outputRoot, layout.Root);
        Assert.AreEqual(Path.Combine(outputRoot, "Fixture"), layout.ApiDirectory);
        Assert.AreEqual(Path.Combine(outputRoot, "Fixture.Backend"), layout.BackendDirectory);
    }

    /// <summary>Alternate output roots still require configured project paths to include project directory names.</summary>
    [TestMethod]
    public void ResolveProjectLayout_RejectsEmptyProjectPath()
    {
        using var workspace = TempWorkspace.Create();

        var exception = Assert.ThrowsException<ArgumentException>(
            () => GeneratedOutput.ResolveProjectLayout(workspace.Root, workspace.Root, "", "Fixture.Backend"));

        StringAssert.Contains(exception.Message, "must include a directory name");
    }
}
