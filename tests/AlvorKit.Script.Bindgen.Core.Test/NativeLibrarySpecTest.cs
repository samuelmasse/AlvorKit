using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

[TestClass]
public sealed class NativeLibrarySpecTest
{
    [TestMethod]
    public void JsonNativeLibrarySpec_LoadsConfigCaseInsensitively()
    {
        using var workspace = TempWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Root, "bindgen.json"), """
            {
              "namespace": "Fixture.Native",
              "apiClass": "FixtureApi",
              "apiSummary": "Fixture API.",
              "backendClass": "FixtureBackend",
              "nativeClass": "FixtureNative",
              "nativeLibrary": "fixture",
              "prefix": "fixture_",
              "workDir": "fixture-work",
              "sourceDir": "fixture-source",
              "header": "fixture.h",
              "apiProject": "generated/Fixture",
              "backendProject": "generated/Fixture.Backend"
            }
            """);

        var config = new FixtureSpec().LoadConfig(workspace.Root);

        Assert.AreEqual(BindgenConfig.CHeaderKind, config.Kind);
        Assert.AreEqual("Fixture.Native", config.Namespace);
        Assert.AreEqual("FixtureApi", config.ApiClass);
        Assert.AreEqual("fixture", config.NativeLibrary);
    }

    private sealed class FixtureSpec() : JsonNativeLibrarySpec("fixture");
}
