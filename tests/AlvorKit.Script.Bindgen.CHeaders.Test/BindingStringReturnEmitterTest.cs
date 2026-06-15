namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingStringReturnEmitterTest
{
    /// <summary>String-return convenience overloads avoid clashing with native-shaped parameter names.</summary>
    [TestMethod]
    public void Emit_AvoidsGeneratedNameCollisions()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new("test_name", "Name", "nint", "nint",
                    [
                        new("value", "int", "int", "", false),
                        new("destination", "int", "int", "", false),
                        new("result", "int", "int", "", false)
                    ],
                    null,
                    ReturnsCString: true)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "out string? value_");
        StringAssert.Contains(overloads, "Span<char> destination_");
        StringAssert.Contains(overloads, "out ReadOnlySpan<char> result_");
    }
}
