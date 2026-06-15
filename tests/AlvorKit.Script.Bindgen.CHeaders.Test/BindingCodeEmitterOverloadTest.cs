namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterOverloadTest
{
    [TestMethod]
    public void Emit_CallbackSetterRootsDelegate()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            [],
            [],
            [new("test_handle", "TestHandle")],
            [new("TestCallback", "void", [])],
            [new(
                "test_set",
                "Set",
                "void",
                "void",
                [
                    new("handle", "TestHandle", "TestHandle", "", false),
                    new("callback", "nint", "nint", "", false, CallbackType: "TestCallback"),
                ],
                null)],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(api, "private Dictionary<(nint Owner, int Slot), Delegate>? rootedCallbacks;");
        StringAssert.Contains(api, "TestCallback? callback");
        StringAssert.Contains(api, "RootCallback(handle.Handle, 0, callback)");
    }

    [TestMethod]
    public void Emit_SpanExtensionsConvertPointerAndSizeParameters()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.SpanExtensions = true;
        var model = new BindingModel([], [], [], [], [new("test_fill", "Fill", "void", "void",
            [new("buffer", "nint", "nint", "", false, IsUntypedPointer: true), new("bufferSize", "nuint", "nuint", "", false, IsSizeT: true)], null)],
            [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var extensions = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestExtensions.cs"));
        StringAssert.Contains(extensions, "public static void Fill<TBuffer>(this Test test, Span<TBuffer> buffer)");
        StringAssert.Contains(extensions, "ByteLength<TBuffer>(buffer)");
        StringAssert.Contains(extensions, "/// <summary>Returns the byte length of an unmanaged span.</summary>");
    }
}
