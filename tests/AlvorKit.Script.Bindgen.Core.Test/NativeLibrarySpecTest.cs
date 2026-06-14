using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers JSON-backed native library specs.</summary>
[TestClass]
public sealed class NativeLibrarySpecTest
{
    /// <summary>JSON config loading is case-insensitive and preserves top-level values.</summary>
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

    /// <summary>Nested config model types keep deserializing after each type is split to its own file.</summary>
    [TestMethod]
    public void JsonNativeLibrarySpec_LoadsNestedBindingHints()
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
              "backendProject": "generated/Fixture.Backend",
              "enumGroups": {
                "FixtureMode": { "prefix": "FIXTURE_MODE_", "flags": true }
              },
              "enumOverloads": {
                "byParamName": { "mode": "FixtureMode" },
                "functions": {
                  "fixture_run": {
                    "return": "FixtureResult",
                    "params": { "mode": [ "FixtureMode", "int" ] }
                  }
                }
              },
              "callbacks": {
                "FIXTUREPROC": {
                  "managedName": "FixtureProc",
                  "paramGroups": { "mode": "FixtureMode" }
                }
              }
            }
            """);

        var config = new FixtureSpec().LoadConfig(workspace.Root);

        Assert.IsTrue(config.EnumGroups["FixtureMode"].Flags);
        Assert.AreEqual("FixtureMode", config.EnumOverloads?.ByParamName["mode"]);
        Assert.AreEqual("FixtureResult", config.EnumOverloads?.Functions["fixture_run"].Return);
        CollectionAssert.AreEqual(new[] { "FixtureMode", "int" }, config.EnumOverloads?.Functions["fixture_run"].Params["mode"]);
        Assert.AreEqual("FixtureProc", config.Callbacks["FIXTUREPROC"].ManagedName);
        Assert.AreEqual("FixtureMode", config.Callbacks["FIXTUREPROC"].ParamGroups["mode"]);
    }
}
