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

    /// <summary>String-return convenience overloads also honor configured typed parameter shapes.</summary>
    [TestMethod]
    public void Emit_AddsTypedStringReturnOverloads()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.EnumOverloads = new() { ByParamName = { ["key"] = "TestKey" } };
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new("test_key_name", "KeyName", "nint", "nint",
                    [new("key", "int", "int", "", false), new("scancode", "int", "int", "", false)],
                    null,
                    ReturnsCString: true)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public void KeyName(int key, int scancode, out string? value)");
        StringAssert.Contains(overloads, "public void KeyName(TestKey key, int scancode, out string? value)");
        StringAssert.Contains(overloads, "value = Marshal.PtrToStringUTF8(KeyName((int)key, scancode));");
        StringAssert.Contains(overloads, "public unsafe void KeyName(TestKey key, int scancode, Span<char> destination, out ReadOnlySpan<char> result)");
    }

    /// <summary>Configured string array returns copy a native char-pointer array to managed strings.</summary>
    [TestMethod]
    public void Emit_AddsStringArrayReturnOverload()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.StringArrayReturns = ["test_extensions", "test_not_extensions"];
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new(
                    "test_extensions",
                    "Extensions",
                    "nint",
                    "nint",
                    [new("count", "uint", "uint", "out", false)],
                    null),
                new(
                    "test_not_extensions",
                    "NotExtensions",
                    "int",
                    "int",
                    [new("count", "uint", "uint", "out", false)],
                    null)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public unsafe string[] Extensions()");
        StringAssert.Contains(overloads, "var pointer = Extensions(out var count);");
        StringAssert.Contains(overloads, "var items = new ReadOnlySpan<nint>((void*)pointer, length);");
        StringAssert.Contains(overloads, "values[index] = Marshal.PtrToStringUTF8(items[index]) ?? \"\";");
        Assert.IsFalse(overloads.Contains("public unsafe string[] NotExtensions", StringComparison.Ordinal));
    }
}
