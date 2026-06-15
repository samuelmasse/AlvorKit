namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingSpanOverloadEmitterTest
{
    /// <summary>Span overload emission handles skipped, lengthless, partial, and multi-pointer candidates.</summary>
    [TestMethod]
    public void Emit_HandlesConfiguredPointerShapes()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.SpanOverloads = true;
        config.SpanParams = new() { ["test_mix"] = ["userdata"] };
        config.SpanSkip = new() { ["test_skip"] = "manual lifetime" };
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new("test_plain", "Plain", "void", "void", [new("value", "int", "int", "", false)], null),
                new("test_raw", "Raw", "void", "void",
                    [new("data", "nint", "nint", "", false, IsUntypedPointer: true)], null),
                new("test_skip", "Skip", "void", "void", PointerAndSize("buffer"), null),
                new("test_sum", "Sum", "int", "int",
                    [new("written", "int", "int", "out", false), .. PointerAndSize("buffer")], null),
                new("test_mix", "Mix", "void", "void",
                    [
                        new("id", "int", "int", "", false),
                        new("input", "nint", "nint", "", false, IsUntypedPointer: true, IsConstPointee: true),
                        new("inputSize", "nuint", "nuint", "", false, IsSizeT: true),
                        new("output", "nint", "nint", "", false, IsUntypedPointer: true),
                        new("outputSize", "nuint", "nuint", "", false, IsSizeT: true),
                        new("userdata", "nint", "nint", "", false, IsUntypedPointer: true)
                    ],
                    null)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        Assert.IsFalse(overloads.Contains("Plain<", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("Raw<", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("Skip<", StringComparison.Ordinal));
        StringAssert.Contains(overloads, "return Sum(out written, (nint)bufferPtr, ByteLength<TBuffer>(buffer));");
        StringAssert.Contains(
            overloads,
            "public void Mix<TInput, TOutput, TUserdata>(int id, ReadOnlySpan<TInput> input, Span<TOutput> output, Span<TUserdata> userdata)");
        StringAssert.Contains(
            overloads,
            "public void Mix<TInput>(int id, ReadOnlySpan<TInput> input, nint output, nuint outputSize, nint userdata) where TInput : unmanaged");
        StringAssert.Contains(overloads, "where TInput : unmanaged");
        StringAssert.Contains(overloads, "where TOutput : unmanaged");
        StringAssert.Contains(overloads, "where TUserdata : unmanaged");
        StringAssert.Contains(
            overloads,
            "Mix(id, (nint)inputPtr, ByteLength<TInput>(input), (nint)outputPtr, ByteLength<TOutput>(output), (nint)userdataPtr);");
    }

    private static List<BindingParameter> PointerAndSize(string name) =>
    [
        new(name, "nint", "nint", "", false, IsUntypedPointer: true),
        new(name + "Size", "nuint", "nuint", "", false, IsSizeT: true)
    ];
}
