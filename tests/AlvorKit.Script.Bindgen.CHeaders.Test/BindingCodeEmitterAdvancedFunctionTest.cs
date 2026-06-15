namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers generated attributes for raw native-shaped methods that should be de-emphasized.</summary>
[TestClass]
public sealed class BindingCodeEmitterAdvancedFunctionTest
{
    /// <summary>Advanced raw binding functions are hidden from normal editor completion in all generated layers.</summary>
    [TestMethod]
    public void Emit_AdvancedFunctionAddsEditorBrowsableAttribute()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var function = new BindingFunction(
            "test_raw",
            "Raw",
            "void",
            "void",
            [new("data", "nint", "nint", "", HasStringConvenience: false)],
            Documentation: null,
            IsAdvanced: true);
        var model = new BindingModel([], [], [], [], [function], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var attribute = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]";
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestWrapper.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestNoop.cs")), attribute);
    }

    /// <summary>Generated API contract, wrapper, and noop scaffolding are excluded from coverage metrics.</summary>
    [TestMethod]
    public void Emit_GeneratedScaffoldingAddsCoverageExclusionAttribute()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel([], [], [], [], [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var attribute = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestWrapper.cs")), attribute);
        StringAssert.Contains(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestNoop.cs")), attribute);
    }
}
