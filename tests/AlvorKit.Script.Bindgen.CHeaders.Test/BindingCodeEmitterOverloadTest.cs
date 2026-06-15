namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterOverloadTest
{
    /// <summary>Callback overloads root delegate parameters while preserving void native return shapes.</summary>
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

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "private Dictionary<(nint Owner, int Slot), Delegate>? rootedCallbacks;");
        StringAssert.Contains(overloads, "TestCallback? callback");
        StringAssert.Contains(overloads, "public void Set(TestHandle handle, TestCallback? callback)");
        StringAssert.Contains(overloads, "RootCallback(handle.Handle, 0, callback)");
    }

    /// <summary>Callback overloads use a zero owner when no handle parameter is available.</summary>
    [TestMethod]
    public void Emit_CallbackSetterWithoutHandleUsesZeroOwner()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            [],
            [],
            [],
            [new("TestCallback", "void", [])],
            [new("test_set", "Set", "void", "void",
                [new("callback", "nint", "nint", "", false, CallbackType: "TestCallback")], null)],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "RootCallback(0, 0, callback)");
    }

    /// <summary>Callback overloads preserve non-void native return values when installing delegates.</summary>
    [TestMethod]
    public void Emit_CallbackSetterPreservesReturnType()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            [],
            [],
            [new("test_handle", "TestHandle")],
            [new("TestCallback", "void", [])],
            [new("test_set", "Set", "int", "int",
                [
                    new("callback", "nint", "nint", "", false, CallbackType: "TestCallback"),
                    new("handle", "TestHandle", "TestHandle", "out", false)
                ],
                null)],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public int Set(TestCallback? callback, out TestHandle handle) => Set(RootCallback(0, 0, callback), out handle);");
    }

    /// <summary>Typed enum overloads can forward to native-sized integer wrapper parameters.</summary>
    [TestMethod]
    public void Emit_TypedOverloadCastsToNativeSizedIntegerWrappers()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.EnumOverloads = new()
        {
            Functions =
            {
                ["test_load"] = new() { Params = { ["flags"] = ["TestFlags"] } },
                ["test_seek"] = new() { Params = { ["offset"] = ["TestOffset"] } }
            }
        };
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new("test_load", "Load", "int", "int", [new("flags", "CULong", "CULong", "", false)], null),
                new("test_seek", "Seek", "int", "int", [new("offset", "CLong", "CLong", "", false)], null)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public int Load(TestFlags flags) => Load(new CULong((uint)flags));");
        StringAssert.Contains(overloads, "public int Seek(TestOffset offset) => Seek(new CLong((int)offset));");
    }

    /// <summary>Span overloads convert raw pointer and size pairs to managed spans.</summary>
    [TestMethod]
    public void Emit_SpanOverloadsConvertPointerAndSizeParameters()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.SpanOverloads = true;
        var model = new BindingModel([], [], [], [], [new("test_fill", "Fill", "void", "void",
            [new("buffer", "nint", "nint", "", false, IsUntypedPointer: true), new("bufferSize", "nuint", "nuint", "", false, IsSizeT: true)], null)],
            [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public unsafe partial class Test");
        StringAssert.Contains(overloads, "/// <inheritdoc cref=\"Test.Fill(nint, nuint)\"/>");
        StringAssert.Contains(
            overloads,
            "/// <remarks>Convenience overload. Pins span arguments for the duration of the call, supplies byte lengths where the native method expects them, " +
            "and forwards to the underlying method.</remarks>");
        StringAssert.Contains(overloads, "public void Fill<TBuffer>(Span<TBuffer> buffer)");
        StringAssert.Contains(overloads, "ByteLength<TBuffer>(buffer)");
        StringAssert.Contains(overloads, "/// <summary>Returns the byte length of an unmanaged span.</summary>");
    }

}
