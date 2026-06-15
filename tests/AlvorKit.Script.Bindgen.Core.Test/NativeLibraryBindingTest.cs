using System.Text.Json;

namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers resolved native library binding paths and validation.</summary>
[TestClass]
public sealed class NativeLibraryBindingTest
{
    /// <summary>C-header bindings compose paths, package identity, and revision-based versions.</summary>
    [TestMethod]
    public void Load_CHeaderBindingBuildsExpectedPathsAndVersion()
    {
        using var workspace = TempWorkspace.Create();
        var repository = CreateRepository(workspace);
        CreateLibrary(repository, "fixture", tag: "1.2.3", revision: "4");
        var config = TestConfig(workspace);
        config.SourceDir = "src-{tag}-{tagDashes}";
        config.IncludeSubdir = "include";
        config.SizeofShim = "src/shim.c";
        WriteConfig(repository, "fixture", config);

        var binding = NativeLibraryBinding.Load(repository, "fixture");

        Assert.AreEqual(repository.Root, binding.RepositoryRoot);
        Assert.AreEqual("fixture", binding.Name);
        Assert.AreEqual("1.2.3.4", binding.Version);
        Assert.AreEqual("1.2.3.4", binding.NativeVersion);
        Assert.AreEqual("1.2.3.4", binding.BindingVersion);
        Assert.AreEqual("4", binding.NativeRevision);
        Assert.AreEqual("4", binding.BindingRevision);
        Assert.AreEqual(config.WorkDir, binding.WorkRoot);
        Assert.AreEqual(Path.Combine(config.WorkDir, "src-1.2.3-1-2-3"), binding.SourceDirectory);
        Assert.AreEqual(Path.Combine(binding.SourceDirectory, "include"), binding.IncludeDirectory);
        Assert.AreEqual(Path.Combine(binding.SourceDirectory, "fixture.h"), binding.HeaderPath);
        Assert.AreEqual(Path.Combine(binding.Directory, "src", "shim.c"), binding.SizeofShimPath);
        Assert.AreEqual("Fixture.Native", binding.NativePackageId);
    }

    /// <summary>GL registry bindings package by GL version and resolve documentation source paths.</summary>
    [TestMethod]
    public void Load_GlRegistryBindingUsesGlVersionAndDocPaths()
    {
        using var workspace = TempWorkspace.Create();
        var repository = CreateRepository(workspace);
        CreateLibrary(repository, "opengl", bindingRevision: "7", writeTag: false);
        var config = TestConfig(workspace);
        config.Kind = BindgenConfig.GlRegistryKind;
        config.NativeClass = "";
        config.NativeLibrary = "";
        config.GlVersion = "4.6";
        config.SourceTag = "abc123";
        config.SourceDir = "registry-{tag}";
        config.DocTag = "doc.1";
        config.DocUrl = "https://example.test/docs-{docTag}.tar.gz";
        config.DocDir = "docs-{docTag}";
        config.DocSubdir = "gl4";
        WriteConfig(repository, "opengl", config);

        var binding = NativeLibraryBinding.Load(repository, "opengl");

        Assert.AreEqual("4.6.7", binding.Version);
        Assert.AreEqual("4.6", binding.NativeVersion);
        Assert.AreEqual("", binding.NativeRevision);
        Assert.AreEqual("7", binding.BindingRevision);
        Assert.AreEqual(Path.Combine(config.WorkDir, "registry-abc123"), binding.SourceDirectory);
        Assert.AreEqual(Path.Combine(config.WorkDir, "docs-doc.1"), binding.DocDirectory);
        Assert.AreEqual(Path.Combine(config.WorkDir, "docs-doc.1", "gl4"), binding.DocReadDirectory);
        Assert.AreEqual("abc123", binding.ReplaceVersionTokens("{tag}"));
        Assert.AreEqual("abc123", binding.ReplaceVersionTokens("{tagDashes}"));
        Assert.AreEqual("doc.1", binding.ReplaceVersionTokens("{docTag}"));
    }

    /// <summary>A separate binding revision changes generated packages without changing native lookup.</summary>
    [TestMethod]
    public void Load_UsesBindingRevisionWhenConfigured()
    {
        using var workspace = TempWorkspace.Create();
        var repository = CreateRepository(workspace);
        CreateLibrary(repository, "fixture", tag: "1.2.3", revision: "4", bindingRevision: "9");
        var config = TestConfig(workspace);
        WriteConfig(repository, "fixture", config);

        var binding = NativeLibraryBinding.Load(repository, "fixture");

        Assert.AreEqual("1.2.3.4", binding.NativeVersion);
        Assert.AreEqual("1.2.3.9", binding.BindingVersion);
        Assert.AreEqual("1.2.3.9", binding.Version);
        Assert.AreEqual("4", binding.NativeRevision);
        Assert.AreEqual("9", binding.BindingRevision);
    }

    /// <summary>Invalid config shapes are rejected before generation starts.</summary>
    [TestMethod]
    public void Load_RejectsInvalidConfigShapes()
    {
        AssertInvalid("unknown", config => config.Kind = "mystery", "unknown bindgen kind");
        AssertInvalid("missing-native", config => config.NativeClass = "", "c-header bindings require nativeClass");
        AssertInvalid("gl-missing-version", config =>
        {
            config.Kind = BindgenConfig.GlRegistryKind;
            config.NativeClass = "";
            config.NativeLibrary = "";
        }, "gl-registry bindings require glVersion");
        AssertInvalid("gl-doc-missing-tag", config =>
        {
            config.Kind = BindgenConfig.GlRegistryKind;
            config.NativeClass = "";
            config.NativeLibrary = "";
            config.GlVersion = "4.6";
            config.DocUrl = "https://example.test/docs.tar.gz";
        }, "docTag is missing");
    }

    /// <summary>Creates a minimal repository layout that RepositoryLayout can discover.</summary>
    private static RepositoryLayout CreateRepository(TempWorkspace workspace)
    {
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        Directory.CreateDirectory(Path.Combine(workspace.Root, "native"));
        return RepositoryLayout.FindFrom(workspace.Root);
    }

    /// <summary>Creates native library marker files under a test repository.</summary>
    private static void CreateLibrary(
        RepositoryLayout repository,
        string name,
        string tag = "1.0.0",
        string revision = "",
        string bindingRevision = "",
        bool writeTag = true)
    {
        var directory = Path.Combine(repository.NativeDirectory, name);
        var conf = Path.Combine(directory, "conf");
        var version = Path.Combine(directory, "version");
        Directory.CreateDirectory(conf);
        Directory.CreateDirectory(version);
        if (writeTag)
            File.WriteAllText(Path.Combine(version, "TAG"), tag);
        if (revision.Length > 0)
            File.WriteAllText(Path.Combine(version, "REVISION"), revision);
        if (bindingRevision.Length > 0)
            File.WriteAllText(Path.Combine(version, "BINDING_REVISION"), bindingRevision);
    }

    /// <summary>Writes a bindgen config fixture for a native library.</summary>
    private static void WriteConfig(RepositoryLayout repository, string name, BindgenConfig config) =>
        File.WriteAllText(
            Path.Combine(repository.NativeDirectory, name, "conf", "bindgen.json"),
            JsonSerializer.Serialize(config));

    /// <summary>Creates a valid C-header config with test-local work directories.</summary>
    private static BindgenConfig TestConfig(TempWorkspace workspace) => new()
    {
        Namespace = "Fixture",
        ApiClass = "FixtureApi",
        ApiSummary = "Fixture API.",
        BackendClass = "FixtureBackend",
        NativeClass = "FixtureNative",
        NativeLibrary = "fixture",
        Prefix = "fixture_",
        WorkDir = workspace.CreateDirectory("work"),
        SourceDir = "source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };

    /// <summary>Asserts that a mutated config fails binding load with an expected message.</summary>
    private static void AssertInvalid(string name, Action<BindgenConfig> mutate, string expectedMessage)
    {
        using var workspace = TempWorkspace.Create();
        var repository = CreateRepository(workspace);
        CreateLibrary(repository, name);
        var config = TestConfig(workspace);
        mutate(config);
        WriteConfig(repository, name, config);

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => NativeLibraryBinding.Load(repository, name));
        StringAssert.Contains(exception.Message, expectedMessage);
    }
}
