namespace AlvorKit.Script.Workspace.Test;

/// <summary>Covers repository config discovery and YAML/JSON loading behavior.</summary>
[TestClass]
public sealed class RepositoryConfigFileTest
{
    /// <summary>YAML loading accepts case-insensitive property names and nested collections.</summary>
    [TestMethod]
    public void Read_LoadsYamlCaseInsensitively()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(
            "conf/bindgen.yml",
            """
            NATIVELIBRARY: sample
            packages:
              - build-essential
            names:
              source: target
            """);

        var config = RepositoryConfigFile.Read<SampleConfig>(workspace.PathFor("conf"), "bindgen");

        Assert.AreEqual("sample", config.NativeLibrary);
        CollectionAssert.AreEqual(new[] { "build-essential" }, config.Packages);
        Assert.AreEqual("target", config.Names["source"]);
    }

    /// <summary>JSON remains readable as a transitional format while repository configs move to YAML.</summary>
    [TestMethod]
    public void Read_LoadsJsonFallback()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(
            "conf/bindgen.json",
            """
            {
              "NATIVELIBRARY": "sample",
              "packages": ["build-essential"],
            }
            """);

        var config = RepositoryConfigFile.Read<SampleConfig>(workspace.PathFor("conf"), "bindgen");

        Assert.AreEqual("sample", config.NativeLibrary);
        CollectionAssert.AreEqual(new[] { "build-essential" }, config.Packages);
    }

    /// <summary>Null JSON content fails with a clear read error while JSON remains a supported fallback format.</summary>
    [TestMethod]
    public void Read_NullJsonDocument_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.Write("conf/native-build.json", "null");

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => RepositoryConfigFile.ReadPath<SampleConfig>(path));

        StringAssert.Contains(exception.Message, "Could not read");
    }

    /// <summary>A directory with two config formats fails clearly instead of letting readers disagree.</summary>
    [TestMethod]
    public void Find_WithDuplicateFormats_Throws()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("conf/bindgen.yml", "nativeLibrary: sample");
        workspace.Write("conf/bindgen.json", """{"nativeLibrary":"sample"}""");

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => RepositoryConfigFile.Find(workspace.PathFor("conf"), "bindgen"));

        StringAssert.Contains(exception.Message, "Multiple config files for bindgen");
    }

    /// <summary>Recursive discovery returns only supported files with the requested config stem.</summary>
    [TestMethod]
    public void FindAll_ReturnsSupportedStemMatches()
    {
        using var workspace = TempWorkspace.Create();
        var alpha = workspace.Write("native/alpha/conf/bindgen.yml", "nativeLibrary: alpha");
        var beta = workspace.Write("native/beta/conf/bindgen.yaml", "nativeLibrary: beta");
        workspace.Write("native/beta/conf/native-build.yml", "kind: single-c");
        workspace.Write("native/gamma/conf/bindgen.txt", "ignored");

        var configs = RepositoryConfigFile.FindAll(workspace.PathFor("native"), "bindgen");

        CollectionAssert.AreEqual(new[] { alpha, beta }, configs.ToArray());
    }

    /// <summary>Null YAML content fails with a clear read error for required manifests.</summary>
    [TestMethod]
    public void Read_NullYamlDocument_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.Write("conf/native-build.yml", "null");

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => RepositoryConfigFile.ReadPath<SampleConfig>(path));

        StringAssert.Contains(exception.Message, "Could not read");
    }

    /// <summary>Unsupported explicit config paths are rejected before parsing.</summary>
    [TestMethod]
    public void ReadPath_UnsupportedExtension_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.Write("conf/bindgen.toml", "nativeLibrary = sample");

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => RepositoryConfigFile.ReadPath<SampleConfig>(path));

        StringAssert.Contains(exception.Message, "Unsupported repository config file");
    }

    /// <summary>Small model used to test repository config loading.</summary>
    private sealed class SampleConfig
    {
        /// <summary>Required scalar value used by repository native configs.</summary>
        public required string NativeLibrary { get; init; }

        /// <summary>Sequence value used by repository native configs.</summary>
        public string[] Packages { get; init; } = [];

        /// <summary>Map value used by repository native configs.</summary>
        public Dictionary<string, string> Names { get; init; } = [];
    }
}
