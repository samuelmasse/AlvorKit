namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers generated attributes and docs for platform-specific native functions.</summary>
[TestClass]
public sealed class BindingCodeEmitterPlatformFunctionTest
{
    /// <summary>Platform-specific functions carry the platform-support attribute in all generated layers.</summary>
    [TestMethod]
    public void Emit_PlatformFunctionAddsSupportedOSPlatformAttribute()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var function = new BindingFunction(
            "test_win",
            "Win",
            "void",
            "void",
            [new("data", "nint", "nint", "", HasStringConvenience: false)],
            Documentation: null,
            Platform: "windows");
        var model = new BindingModel([], [], [], [], [function], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var attribute = "[global::System.Runtime.Versioning.SupportedOSPlatform(\"windows\")]";
        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(api, attribute);
        StringAssert.Contains(api, "Platform-specific: only windows builds");
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestWrapper.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestNoop.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestNative.cs")), attribute);
    }

    /// <summary>Functions without a platform label emit no platform-support attribute anywhere.</summary>
    [TestMethod]
    public void Emit_UnlabelledFunctionEmitsNoPlatformAttribute()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var function = new BindingFunction(
            "test_everywhere",
            "Everywhere",
            "void",
            "void",
            [],
            Documentation: null);
        var model = new BindingModel([], [], [], [], [function], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        Assert.IsFalse(api.Contains("SupportedOSPlatform", StringComparison.Ordinal));
        Assert.IsFalse(api.Contains("Platform-specific", StringComparison.Ordinal));
    }

    /// <summary>Convenience overloads keep the platform-support attribute so callers get analyzer coverage.</summary>
    [TestMethod]
    public void Emit_PlatformFunctionAddsAttributeToStringReturnOverloads()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var function = new BindingFunction(
            "test_get_name",
            "GetName",
            "nint",
            "nint",
            [],
            Documentation: null,
            ReturnsCString: true,
            Platform: "macos");
        var model = new BindingModel([], [], [], [], [function], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        var attribute = "[global::System.Runtime.Versioning.SupportedOSPlatform(\"macos\")]";
        Assert.AreEqual(2, overloads.Split(attribute).Length - 1, "both string-return overloads carry the attribute");
    }
}
