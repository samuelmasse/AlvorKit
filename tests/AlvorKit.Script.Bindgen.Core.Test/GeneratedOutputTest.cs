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
}
