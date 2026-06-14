using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for loading resolved library build metadata.</summary>
[TestClass]
public sealed class LibraryBuildContextTest
{
    /// <summary>Library metadata loads package versions and derived paths.</summary>
    [TestMethod]
    public void Load_ReadsVersionAndPaths()
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        try
        {
            var context = LibraryBuildContext.Load(new(root), "sample");

            Assert.AreEqual("1.2.3.2", context.NativeVersion);
            StringAssert.EndsWith(context.SourceDirectory, Path.Combine(workDir, "src-1.2.3"));
            StringAssert.EndsWith(context.OutputFile(TargetRid.Parse("linux-x64")), Path.Combine("runtimes", "linux-x64", "native", "libsample.so"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Repository layout lists only directories with native-build manifests.</summary>
    [TestMethod]
    public void NativeBuildLibraries_ReturnsManifestDirectories()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            Assert.AreEqual("sample", string.Join(",", new RepositoryLayout(root).NativeBuildLibraries()));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
