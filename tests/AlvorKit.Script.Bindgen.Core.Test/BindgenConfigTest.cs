namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers defaults on the bindgen config model.</summary>
[TestClass]
public sealed class BindgenConfigTest
{
    /// <summary>Required config fields can be supplied while optional generation knobs keep safe defaults.</summary>
    [TestMethod]
    public void Defaults_KeepOptionalGenerationKnobsEmpty()
    {
        var config = TestConfig();

        Assert.AreEqual(BindgenConfig.CHeaderKind, config.Kind);
        Assert.AreEqual("Num", config.DigitNamePrefix);
        Assert.AreEqual("gl", config.GlApi);
        Assert.AreEqual("core", config.GlProfile);
        Assert.AreEqual("gles2", config.GlEsApi);
        Assert.AreEqual(0, config.Constants.Count);
        Assert.AreEqual(0, config.BoolReturns.Length);
        Assert.AreEqual(0, config.EnumGroups.Count);
        Assert.IsNull(config.EnumOverloads);
    }

    /// <summary>Creates the smallest valid C-header config used by defaults tests.</summary>
    private static BindgenConfig TestConfig() => new()
    {
        Namespace = "Fixture.Native",
        ApiClass = "FixtureApi",
        ApiSummary = "Fixture API.",
        BackendClass = "FixtureBackend",
        NativeClass = "FixtureNative",
        NativeLibrary = "fixture",
        Prefix = "fixture_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };
}
