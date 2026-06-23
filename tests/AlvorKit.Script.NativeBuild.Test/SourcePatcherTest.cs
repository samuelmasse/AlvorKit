namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for manifest-driven source archive patches.</summary>
[TestClass]
public sealed class SourcePatcherTest
{
    /// <summary>Source patches replace configured text inside extracted source files.</summary>
    [TestMethod]
    public void Apply_WhenSearchTextExists_ReplacesText()
    {
        var context = LoadPatchedContext(out var root, out var sourceFile);
        try
        {
            SourcePatcher.Apply(context);

            Assert.AreEqual("before patched after", File.ReadAllText(sourceFile));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Source patches are idempotent when the replacement text is already present.</summary>
    [TestMethod]
    public void Apply_WhenReplacementAlreadyExists_DoesNothing()
    {
        var context = LoadPatchedContext(out var root, out var sourceFile);
        try
        {
            File.WriteAllText(sourceFile, "before patched after");

            SourcePatcher.Apply(context);

            Assert.AreEqual("before patched after", File.ReadAllText(sourceFile));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Source patches fail when neither original nor replacement text is present.</summary>
    [TestMethod]
    public void Apply_WhenSearchAndReplacementAreMissing_Throws()
    {
        var context = LoadPatchedContext(out var root, out var sourceFile);
        try
        {
            File.WriteAllText(sourceFile, "before other after");

            var exception = Assert.ThrowsException<InvalidOperationException>(() => SourcePatcher.Apply(context));

            StringAssert.Contains(exception.Message, "source patch search text was not found");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Creates a loaded library context with one patchable extracted source file.</summary>
    private static LibraryBuildContext LoadPatchedContext(out string root, out string sourceFile)
    {
        root = TestRepositoryFactory.CreateCMakeLibrary("sample", "sample-work");
        var conf = Path.Combine(root, "native", "sample", "conf");
        File.WriteAllText(Path.Combine(conf, "native-build.yml"), """
            kind: cmake
            sourcePatches:
              - path: src/sample.txt
                search: original
                replace: patched
            linux:
              cmakeOutput: src/libsample.so
            """);

        var context = LibraryBuildContext.Load(new(root), "sample");
        var sourceDirectory = Path.Combine(context.SourceDirectory, "src");
        Directory.CreateDirectory(sourceDirectory);
        sourceFile = Path.Combine(sourceDirectory, "sample.txt");
        File.WriteAllText(sourceFile, "before original after");
        return context;
    }
}
