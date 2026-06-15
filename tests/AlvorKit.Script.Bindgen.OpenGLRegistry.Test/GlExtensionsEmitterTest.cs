namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for generated OpenGL convenience overload source.</summary>
[TestClass]
public sealed class GlExtensionsEmitterTest
{
    /// <summary>Combined overloads infer byte counts and pin generic spans for void pointers.</summary>
    [TestMethod]
    public void Emit_CombinedGenericSpanOverloadInfersByteLength()
    {
        var model = ModelWithCommands([
            new(
                NativeName: "glBufferData",
                ManagedName: "BufferData",
                ReturnType: "void",
                ReturnInteropType: "void",
                Parameters:
                [
                    new("size", "size", "nint", "nint", Len: null, PointerDepth: 0, PointeeType: null, PointeeIsConst: false, PointeeIsChar: false),
                    new("data", "data", "nint", "nint", Len: "size", PointerDepth: 1, PointeeType: null, PointeeIsConst: true, PointeeIsChar: false)
                ],
                Availability: new("1.0", null),
                Documentation: null,
                ReturnsCString: false)
        ]);

        var source = new GlExtensionsEmitter(OpenGlRegistryTestConfig.Create()).Emit(model, Header())!;

        StringAssert.Contains(source, "public virtual void BufferData<T>(ReadOnlySpan<T> data) where T : unmanaged");
        StringAssert.Contains(source, "ByteLength<T>(data)");
        StringAssert.Contains(source, "fixed (T* dataPtr = data)");
    }

    /// <summary>Info-log helpers emit allocating string and caller-buffer overloads.</summary>
    [TestMethod]
    public void Emit_InfoLogOverloadsGrowUtf8Buffers()
    {
        var model = ModelWithCommands([
            new(
                NativeName: "glGetShaderInfoLog",
                ManagedName: "GetShaderInfoLog",
                ReturnType: "void",
                ReturnInteropType: "void",
                Parameters:
                [
                    new("shader", "shader", "uint", "uint", Len: null, PointerDepth: 0, PointeeType: null, PointeeIsConst: false, PointeeIsChar: false),
                    new("bufSize", "bufSize", "int", "int", Len: null, PointerDepth: 0, PointeeType: null, PointeeIsConst: false, PointeeIsChar: false),
                    new("length", "length", "nint", "nint", Len: "1", PointerDepth: 1, PointeeType: "int", PointeeIsConst: false, PointeeIsChar: false),
                    new("infoLog", "infoLog", "nint", "nint", Len: "bufSize", PointerDepth: 1, PointeeType: "byte", PointeeIsConst: false, PointeeIsChar: true)
                ],
                Availability: new("1.0", null),
                Documentation: null,
                ReturnsCString: false)
        ]);

        var source = new GlExtensionsEmitter(OpenGlRegistryTestConfig.Create()).Emit(model, Header())!;

        StringAssert.Contains(source, "public virtual string GetShaderInfoLog(uint shader)");
        StringAssert.Contains(source, "return Encoding.UTF8.GetString(buffer[..written]);");
        StringAssert.Contains(source, "public virtual ReadOnlySpan<char> GetShaderInfoLog(uint shader, Span<char> destination)");
    }

    /// <summary>Combined overloads marshal strings, string arrays, typed spans, and configured unsized generic spans.</summary>
    [TestMethod]
    public void Emit_CombinedOverloadsCoverStringAndSpanShapes()
    {
        var config = OpenGlRegistryTestConfig.Create();
        config.SpanParams["glNamedBufferSubData"] = ["data"];
        var model = ModelWithCommands([
            Command(
                "glObjectLabel",
                "ObjectLabel",
                [
                    Parameter("identifier", "identifier", "GlObjectIdentifier", "uint"),
                    Parameter("name", "name", "uint", "uint"),
                    Parameter("length", "length", "int", "int"),
                    Parameter("label", "label", "string", "nint", "length", 1, "byte", true, true)
                ]),
            Command(
                "glTransformFeedbackVaryings",
                "TransformFeedbackVaryings",
                [
                    Parameter("program", "program", "uint", "uint"),
                    Parameter("count", "count", "int", "int"),
                    Parameter("varyings", "varyings", "IReadOnlyList<string>", "nint", "count", 2, "byte", true, true),
                    Parameter("bufferMode", "bufferMode", "GlEnum", "uint")
                ]),
            Command(
                "glDrawBuffers",
                "DrawBuffers",
                [
                    Parameter("n", "n", "int", "int"),
                    Parameter("bufs", "bufs", "GlDrawBufferMode", "nint", "n", 1, "GlDrawBufferMode", true, false)
                ]),
            Command(
                "glReadPixels",
                "ReadPixels",
                [
                    Parameter("size", "size", "nint", "nint"),
                    Parameter("pixels", "pixels", "nint", "nint", "size", 1, null, false, false)
                ]),
            Command(
                "glNamedBufferSubData",
                "NamedBufferSubData",
                [
                    Parameter("buffer", "buffer", "uint", "uint"),
                    Parameter("data", "data", "nint", "nint", null, 1, null, true, false)
                ])
        ]);

        var source = new GlExtensionsEmitter(config).Emit(model, Header())!;

        StringAssert.Contains(source, "public virtual void ObjectLabel(GlObjectIdentifier identifier, uint name, int length, string label)");
        StringAssert.Contains(source, "using var labelUtf8 = new Utf8(label, stackalloc byte[256]);");
        StringAssert.Contains(source, "public virtual void TransformFeedbackVaryings(uint program, ReadOnlySpan<string> varyings, GlEnum bufferMode)");
        StringAssert.Contains(source, "using var varyingsArray = new Utf8Array(varyings, stackalloc nint[128]);");
        StringAssert.Contains(source, "public virtual void DrawBuffers(ReadOnlySpan<GlDrawBufferMode> bufs)");
        StringAssert.Contains(source, "fixed (GlDrawBufferMode* bufsPtr = bufs)");
        StringAssert.Contains(source, "public virtual void ReadPixels<T>(Span<T> pixels) where T : unmanaged");
        StringAssert.Contains(source, "public virtual void NamedBufferSubData<T>(uint buffer, ReadOnlySpan<T> data) where T : unmanaged");
    }

    /// <summary>Specialized overload emitters cover singular, out-scalar, and single-source helper shapes.</summary>
    [TestMethod]
    public void Emit_SpecializedOverloadsCoverSingularOutScalarAndSingleSourceShapes()
    {
        var model = ModelWithCommands([
            Command(
                "glGenBuffers",
                "GenBuffers",
                [
                    Parameter("n", "n", "int", "int"),
                    Parameter("buffers", "buffers", "nint", "nint", "n", 1, "uint", false, false)
                ]),
            Command(
                "glDeleteBuffers",
                "DeleteBuffers",
                [
                    Parameter("n", "n", "int", "int"),
                    Parameter("buffers", "buffers", "nint", "nint", "n", 1, "uint", true, false)
                ]),
            Command(
                "glGetIntegerv",
                "GetIntegerv",
                [
                    Parameter("pname", "pname", "GlEnum", "uint"),
                    Parameter("data", "data", "nint", "nint", "1", 1, "int", false, false)
                ]),
            Command(
                "glShaderSource",
                "ShaderSource",
                [
                    Parameter("shader", "shader", "uint", "uint"),
                    Parameter("count", "count", "int", "int"),
                    Parameter("string", "strings", "nint", "nint", "count", 2, "byte", true, true),
                    Parameter("length", "lengths", "nint", "nint", "count", 1, "int", true, false)
                ])
        ]);

        var source = new GlExtensionsEmitter(OpenGlRegistryTestConfig.Create()).Emit(model, Header())!;

        StringAssert.Contains(source, "public virtual uint GenBuffer()");
        StringAssert.Contains(source, "return buffer;");
        StringAssert.Contains(source, "public virtual void DeleteBuffer(uint buffer)");
        StringAssert.Contains(source, "public virtual void GetIntegerv(GlEnum pname, out int data)");
        StringAssert.Contains(source, "public virtual void ShaderSource(uint shader, string source)");
        StringAssert.Contains(source, "Marshals <paramref name=\"source\"/> to UTF-8");
    }

    /// <summary>Combined string overloads drop COMPSIZE length parameters when the string length can supply them.</summary>
    [TestMethod]
    public void Emit_CombinedStringOverloadDropsCompSizeLength()
    {
        var model = ModelWithCommands([
            Command(
                "glObjectLabel",
                "ObjectLabel",
                [
                    Parameter("identifier", "identifier", "GlObjectIdentifier", "uint"),
                    Parameter("name", "name", "uint", "uint"),
                    Parameter("length", "length", "int", "int"),
                    Parameter("label", "label", "string", "nint", "COMPSIZE(length)", 1, "byte", true, true)
                ])
        ]);

        var source = new GlExtensionsEmitter(OpenGlRegistryTestConfig.Create()).Emit(model, Header())!;

        StringAssert.Contains(source, "public virtual void ObjectLabel(GlObjectIdentifier identifier, uint name, string label)");
        StringAssert.Contains(source, "ObjectLabel(identifier, name, labelUtf8.Length, labelUtf8.Pointer);");
    }

    /// <summary>API contract emission adds callback root storage and typed callback setter overloads.</summary>
    [TestMethod]
    public void ApiContractEmitter_WithCallbackParameter_EmitsCallbackSetter()
    {
        var context = new GlCodeEmissionContext(OpenGlRegistryTestConfig.Create(), "registry-tag", "doc-tag");
        var model = ModelWithCommands([
            Command(
                "glDebugMessageCallback",
                "DebugMessageCallback",
                [
                    Parameter("callback", "callback", "GlDebugProc?", "nint", CallbackType: "GlDebugProc"),
                    Parameter("userParam", "userParam", "nint", "nint")
                ])
        ]);

        var source = new GlApiContractEmitter(context).Emit(model);

        StringAssert.Contains(source, "private Dictionary<int, Delegate>? rootedCallbacks");
        StringAssert.Contains(source, "public virtual void DebugMessageCallback(GlDebugProc? callback, nint userParam)");
        StringAssert.Contains(source, "RootCallback(0, callback)");
    }

    /// <summary>Creates a minimal binding model with the supplied commands.</summary>
    private static GlBindingModel ModelWithCommands(IReadOnlyList<GlCommand> commands) =>
        new([], new("GLenum", "GlEnum", IsFlags: false, Members: []), null, commands, [], [], [], []);

    /// <summary>Creates a generated C# source header for direct extension emitter tests.</summary>
    private static StringBuilder Header() => new StringBuilder().AppendLine("// <auto-generated/>").AppendLine("#nullable enable").AppendLine();

    /// <summary>Creates a command with the standard void return and OpenGL 1.0 availability.</summary>
    private static GlCommand Command(string nativeName, string managedName, IReadOnlyList<GlParameter> parameters) =>
        new(nativeName, managedName, "void", "void", parameters, new("1.0", null), Documentation: null, ReturnsCString: false);

    /// <summary>Creates a scalar OpenGL command parameter.</summary>
    private static GlParameter Parameter(
        string nativeName,
        string managedName,
        string managedType,
        string interopType,
        string? len = null,
        int pointerDepth = 0,
        string? pointeeType = null,
        bool pointeeIsConst = false,
        bool pointeeIsChar = false,
        string? CallbackType = null) =>
        new(nativeName, managedName, managedType, interopType, len, pointerDepth, pointeeType, pointeeIsConst, pointeeIsChar, CallbackType);
}
