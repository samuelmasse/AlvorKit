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

    /// <summary>macOS verification checks architecture with file and then lists dynamic library dependencies.</summary>
    [TestMethod]
    public async Task VerifyAsync_MacOSMatchingArchitecture_RunsFileAndOtool()
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        try
        {
            var context = LibraryBuildContext.Load(new(root), "sample");
            var processRunner = new RecordingProcessRunner { CaptureOutput = "Mach-O 64-bit dynamically linked shared library x86_64\n" };

            await NativeBuildVerifier.VerifyAsync(context, TargetRid.Parse("osx-x64"), processRunner, context.Build.MacOS);

            Assert.AreEqual("file,otool", string.Join(",", processRunner.CaptureCommands.Select(command => command.FileName)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>macOS verification rejects binaries whose file output lacks the requested architecture.</summary>
    [TestMethod]
    public async Task VerifyAsync_MacOSWrongArchitecture_Throws()
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        try
        {
            var context = LibraryBuildContext.Load(new(root), "sample");
            var processRunner = new RecordingProcessRunner { CaptureOutput = "Mach-O 64-bit dynamically linked shared library arm64\n" };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => NativeBuildVerifier.VerifyAsync(context, TargetRid.Parse("osx-x64"), processRunner, context.Build.MacOS));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
