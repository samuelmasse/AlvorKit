using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers native source resolver paths that do not require network access.</summary>
[TestClass]
public sealed class NativeSourceResolverTest
{
    /// <summary>An existing header satisfies the source resolver without fetching anything.</summary>
    [TestMethod]
    public async Task EnsureSourceAsync_ReturnsWhenHeaderExists()
    {
        using var workspace = TempWorkspace.Create();
        var binding = CreateBinding(workspace);
        Directory.CreateDirectory(binding.SourceDirectory);
        File.WriteAllText(binding.HeaderPath, "/* ready */");

        await new NativeSourceResolver().EnsureSourceAsync(binding);

        Assert.IsTrue(File.Exists(binding.HeaderPath));
    }

    /// <summary>A missing header without sourceUrl fails with a clear file error.</summary>
    [TestMethod]
    public async Task EnsureSourceAsync_ThrowsWhenMissingHeaderAndNoSourceUrl()
    {
        using var workspace = TempWorkspace.Create();
        var binding = CreateBinding(workspace);

        var exception = await Assert.ThrowsExceptionAsync<FileNotFoundException>(
            () => new NativeSourceResolver().EnsureSourceAsync(binding));
        StringAssert.Contains(exception.Message, "no sourceUrl configured");
    }

    /// <summary>Unconfigured documentation sources are treated as already satisfied.</summary>
    [TestMethod]
    public async Task EnsureDocSourceAsync_ReturnsWhenDocsAreUnconfigured()
    {
        using var workspace = TempWorkspace.Create();
        var binding = CreateBinding(workspace);

        await new NativeSourceResolver().EnsureDocSourceAsync(binding);

        Assert.IsNull(binding.DocReadDirectory);
    }

    /// <summary>An existing doc read directory satisfies the documentation resolver without fetching.</summary>
    [TestMethod]
    public async Task EnsureDocSourceAsync_ReturnsWhenDocDirectoryExists()
    {
        using var workspace = TempWorkspace.Create();
        var binding = CreateBinding(workspace, config =>
        {
            config.DocUrl = "https://example.test/docs.tar.gz";
            config.DocDir = "docs";
            config.DocSubdir = "gl4";
        });
        Directory.CreateDirectory(binding.DocReadDirectory!);

        await new NativeSourceResolver().EnsureDocSourceAsync(binding);

        Assert.IsTrue(Directory.Exists(binding.DocReadDirectory));
    }

    /// <summary>Creates a binding with test-local work and source directories.</summary>
    private static NativeLibraryBinding CreateBinding(TempWorkspace workspace, Action<BindgenConfig>? configure = null)
    {
        File.WriteAllText(Path.Combine(workspace.Root, "AlvorKit.slnx"), "");
        var native = Path.Combine(workspace.Root, "native", "fixture");
        Directory.CreateDirectory(native);
        File.WriteAllText(Path.Combine(native, "TAG"), "1.0.0");
        var repository = RepositoryLayout.FindFrom(workspace.Root);
        var config = TestConfig(workspace);
        configure?.Invoke(config);
        return NativeLibraryBinding.Load(repository, new MemoryNativeLibrarySpec("fixture", config));
    }

    /// <summary>Creates a valid C-header config for resolver tests.</summary>
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
}
