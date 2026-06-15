namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for native binary verification script generation.</summary>
[TestClass]
public sealed class NativeBuildVerifierTest
{
    /// <summary>Windows verification must load the VS dev shell before calling dumpbin.</summary>
    [TestMethod]
    public void WindowsVerifyScript_LoadsVisualStudioShellBeforeDumpbin()
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        try
        {
            var context = LibraryBuildContext.Load(new(root), "sample");
            var script = NativeBuildVerifier.WindowsVerifyScript(context, TargetRid.Parse("win-x64"));

            StringAssert.Contains(script, "Launch-VsDevShell.ps1");
            StringAssert.Contains(script, "dumpbin /nologo /dependents");
            StringAssert.Contains(script, context.OutputFile(TargetRid.Parse("win-x64")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
