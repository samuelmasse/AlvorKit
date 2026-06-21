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
            WideTokens: null,
            Commands: [],
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
                        new(
                            "source",
                            "source",
                            "uint",
                            "uint",
                            Len: null,
                            PointerDepth: 0,
                            PointeeType: null,
                            PointeeIsConst: false,
                            PointeeIsChar: false)
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
            WideTokens: new(
                NativeName: "GLwide",
                ManagedName: "GlWideEnum",
                IsFlags: false,
                Members: [new("TimeoutIgnored", "GL_TIMEOUT_IGNORED", ulong.MaxValue, availability, ["GlSpecialNumbers"])])
            { UnderlyingType = "ulong" },
            Commands: [],
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
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl", "GlWideEnum.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "Gl", "GlConstants.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "Gl.Backend", "Gl.Backend.csproj")));
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, config.ApiProject)));

        var wideEnum = File.ReadAllText(Path.Combine(outputRoot, "Gl", "GlWideEnum.cs"));
        StringAssert.Contains(wideEnum, "public enum GlWideEnum : ulong");
        StringAssert.Contains(wideEnum, "Native OpenGL token constants from <c>gl.xml</c> whose values are too wide");
        StringAssert.Contains(wideEnum, "<c>GL_TIMEOUT_IGNORED</c> (GL 1.0). See <see cref=\"GlSpecialNumbers\"/>.");
        StringAssert.Contains(wideEnum, "TimeoutIgnored = 0xFFFFFFFFFFFFFFFF,");

        var catchAllEnum = File.ReadAllText(Path.Combine(outputRoot, "Gl", "GlEnum.cs"));
        StringAssert.Contains(catchAllEnum, "Native OpenGL token constants from <c>gl.xml</c>");
        StringAssert.Contains(catchAllEnum, "<c>GL_TEXTURE_2D</c> (GL 1.0).");

        var textureTarget = File.ReadAllText(Path.Combine(outputRoot, "Gl", "TextureTarget.cs"));
        StringAssert.Contains(textureTarget, "OpenGL tokens from the <c>TextureTarget</c> registry group.");
        StringAssert.Contains(textureTarget, "<c>GL_TEXTURE_2D</c> (GL 1.0).");

        var handles = File.ReadAllText(Path.Combine(outputRoot, "Gl", "GlHandles.cs"));
        StringAssert.Contains(handles, "Strongly typed wrapper for any <c>GLuint</c> OpenGL object name from <c>gl.xml</c>");
        StringAssert.Contains(handles, "Raw <c>GLuint</c> OpenGL object name.");
    }

    /// <summary>Generated OpenGL API contract, wrapper, and noop scaffolding are excluded from coverage metrics.</summary>
    [TestMethod]
    public void Emit_GeneratedScaffoldingAddsCoverageExclusionAttribute()
    {
        using var workspace = TempWorkspace.Create();
        var config = OpenGlRegistryTestConfig.Create();
        var model = new GlBindingModel(
            Groups: [],
            AllTokens: new("GLenum", "GlEnum", IsFlags: false, Members: []),
            WideTokens: null,
            Commands: [],
            UngroupedEnumUses: [],
            SkippedCommands: [],
            HandleTypes: [],
            Delegates: []);

        new GlCodeEmitter(config, "registry-tag", "doc-tag").Emit(model, workspace.Root, "4.6.3");

        var attribute = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Gl.cs"));
        var wrapper = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "GlWrapper.cs"));
        var noop = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "GlNoop.cs"));
        var backend = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "GlBackend.cs"));

        StringAssert.Contains(api, attribute);
        StringAssert.Contains(api, "native OpenGL commands from <c>gl.xml</c>");
        StringAssert.Contains(wrapper, attribute);
        StringAssert.Contains(wrapper, "native OpenGL commands from <c>gl.xml</c>");
        StringAssert.Contains(wrapper, "The <c>gl.xml</c> OpenGL API instance each call is forwarded to.");
        StringAssert.Contains(noop, attribute);
        StringAssert.Contains(noop, "native OpenGL commands from <c>gl.xml</c>");
        StringAssert.Contains(backend, "native OpenGL command entry points such as <c>glActiveTexture</c>");
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
            WideTokens: null,
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
