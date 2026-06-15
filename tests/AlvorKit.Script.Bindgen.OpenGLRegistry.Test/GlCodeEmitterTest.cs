namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for generated OpenGL code-emission artifacts.</summary>
[TestClass]
public sealed class GlCodeEmitterTest
{
    /// <summary>Generated callback delegates use the Winapi unmanaged calling convention.</summary>
    [TestMethod]
    public void EmitDelegate_UsesWinapiCallingConventionForApiEntryCallbacks()
    {
        using var workspace = TempWorkspace.Create();
        var config = OpenGlRegistryTestConfig.Create();
        var model = new GlBindingModel(
            Groups: [],
            AllTokens: new("GLenum", "GlEnum", IsFlags: false, Members: []),
            Commands: [],
            WideConstants: [],
            UngroupedEnumUses: [],
            SkippedCommands: [],
            HandleTypes: [],
            Delegates:
            [
                new(
                    NativeName: "GLDEBUGPROC",
                    ManagedName: "GlDebugProc",
                    ReturnType: "void",
                    Parameters:
                    [
                        new("source", "source", "uint", "uint", Len: null, PointerDepth: 0, PointeeType: null, PointeeIsConst: false, PointeeIsChar: false)
                    ])
            ]);

        new GlCodeEmitter(config, "registry-tag", "doc-tag").Emit(model, workspace.Root, "4.6.3");

        var delegateSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "GlDebugProc.cs"));
        StringAssert.Contains(delegateSource, "[UnmanagedFunctionPointer(CallingConvention.Winapi)]");
    }

    /// <summary>Alternate output roots receive generated OpenGL projects directly under the requested snapshot directory.</summary>
    [TestMethod]
    public void Emit_CanWriteToAlternateOutputRoot()
    {
        using var workspace = TempWorkspace.Create();
        var config = OpenGlRegistryTestConfig.Create();
        var availability = new GlAvailability("1.0", null);
        var model = new GlBindingModel(
            Groups:
            [
                new(
                    NativeName: "TextureTarget",
                    ManagedName: "TextureTarget",
                    IsFlags: false,
                    Members: [new("Texture2D", "GL_TEXTURE_2D", 0x0DE1, availability, ["TextureTarget"])])
            ],
            AllTokens: new("GLenum", "GlEnum", IsFlags: false, Members: [new("Texture2D", "GL_TEXTURE_2D", 0x0DE1, availability, [])]),
            Commands: [],
            WideConstants: [new("TimeoutIgnored", "GL_TIMEOUT_IGNORED", ulong.MaxValue, availability)],
            UngroupedEnumUses: [],
            SkippedCommands: [],
            HandleTypes: ["GlHandle", "GlTextureHandle"],
            Delegates: []);
        var outputRoot = Path.Combine(workspace.Root, "out", "bindgen-review", "after");

        new GlCodeEmitter(config, "registry-tag", "doc-tag").Emit(model, workspace.Root, outputRoot, "4.6.3");

        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Directory.Build.props")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl", "Gl.csproj")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl", "TextureTarget.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl", "GlHandles.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl", "GlConstants.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl.Backend", "Gl.Backend.csproj")));
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, config.ApiProject)));
    }

    /// <summary>Generated C-string span overloads handle null native string pointers.</summary>
    [TestMethod]
    public void EmitStringGetter_SpanOverloadHandlesNullNativePointer()
    {
        using var workspace = TempWorkspace.Create();
        var config = OpenGlRegistryTestConfig.Create();
        var model = new GlBindingModel(
            Groups: [],
            AllTokens: new("GLenum", "GlEnum", IsFlags: false, Members: []),
            Commands:
            [
                new(
                    NativeName: "glGetString",
                    ManagedName: "GetString",
                    ReturnType: "nint",
                    ReturnInteropType: "nint",
                    Parameters: [],
                    Availability: new("1.0", null),
                    Documentation: null,
                    ReturnsCString: true)
            ],
            WideConstants: [],
            UngroupedEnumUses: [],
            SkippedCommands: [],
            HandleTypes: [],
            Delegates: []);

        new GlCodeEmitter(config, "registry-tag", "doc-tag").Emit(model, workspace.Root, "4.6.3");

        var extensionsSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "GlExtensions.cs"));
        StringAssert.Contains(extensionsSource, "var pointer = this.GetString();");
        StringAssert.Contains(extensionsSource, "if (pointer == 0) { result = default; return; }");
        StringAssert.Contains(extensionsSource, "MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)pointer)");
    }

}
