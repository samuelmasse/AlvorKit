namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers generated translation unit file placement and cleanup.</summary>
[TestClass]
public sealed class TranslationUnitWriterTest
{
    /// <summary>Translation units are created under the binding work root and can be cleaned after parsing.</summary>
    [TestMethod]
    public void Write_CreatesRepoLocalFileAndDeleteRemovesDirectory()
    {
        using var workspace = TempWorkspace.Create();
        var binding = CreateBinding(workspace);

        var path = new TranslationUnitWriter().Write(binding);
        var directory = Path.GetDirectoryName(path)!;

        Assert.IsTrue(File.Exists(path));
        Assert.IsTrue(IsInside(path, binding.WorkRoot));

        TranslationUnitWriter.Delete(path);

        Assert.IsFalse(Directory.Exists(directory));
    }

    /// <summary>Creates a minimal binding metadata fixture.</summary>
    private static NativeLibraryBinding CreateBinding(TempWorkspace workspace)
    {
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native", "fixture");
        var conf = Path.Combine(native, "conf");
        var version = Path.Combine(native, "version");
        Directory.CreateDirectory(conf);
        Directory.CreateDirectory(version);
        File.WriteAllText(Path.Combine(version, "TAG"), "1.0.0");

        var config = CHeaderTestConfig.Create();
        RepositoryConfigFixture.WriteYamlMapping(
            Path.Combine(conf, "bindgen.yml"),
            ("namespace", config.Namespace),
            ("apiClass", config.ApiClass),
            ("apiSummary", config.ApiSummary),
            ("backendClass", config.BackendClass),
            ("nativeClass", config.NativeClass),
            ("nativeLibrary", config.NativeLibrary),
            ("prefix", config.Prefix),
            ("workDir", config.WorkDir),
            ("sourceDir", config.SourceDir),
            ("header", config.Header),
            ("apiProject", config.ApiProject),
            ("backendProject", config.BackendProject));

        return NativeLibraryBinding.Load(RepositoryLayout.FindFrom(workspace.Root), "fixture");
    }

    /// <summary>Returns whether a path resolves under a root directory.</summary>
    private static bool IsInside(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative != ".."
            && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relative);
    }
}
