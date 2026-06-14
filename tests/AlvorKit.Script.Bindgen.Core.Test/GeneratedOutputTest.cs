using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

[TestClass]
public sealed class GeneratedOutputTest
{
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

    [TestMethod]
    public void EmitSharedProps_CarriesGeneratedProjectDefaults()
    {
        var props = GeneratedOutput.EmitSharedProps();

        StringAssert.Contains(props, "<TargetFramework>net10.0</TargetFramework>");
        StringAssert.Contains(props, "<GenerateDocumentationFile>true</GenerateDocumentationFile>");
        StringAssert.Contains(props, "<NoWarn>$(NoWarn);CS1573;CS1591</NoWarn>");
    }
}
