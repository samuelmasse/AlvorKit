using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

[TestClass]
public sealed class GlCodeEmitterTest
{
    [TestMethod]
    public void EmitDelegate_UsesWinapiCallingConventionForApiEntryCallbacks()
    {
        using var workspace = TempWorkspace.Create();
        var config = TestConfig();
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

    [TestMethod]
    public void EmitStringGetter_SpanOverloadHandlesNullNativePointer()
    {
        using var workspace = TempWorkspace.Create();
        var config = TestConfig();
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

    private static BindgenConfig TestConfig() => new()
    {
        Kind = BindgenConfig.GlRegistryKind,
        Namespace = "AlvorKit.Bindgen.OpenGLFixture",
        ApiClass = "Gl",
        ApiSummary = "Fixture GL API.",
        BackendClass = "GlBackend",
        Prefix = "GL_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "gl.xml",
        ApiProject = "generated/Gl",
        BackendProject = "generated/Gl.Backend",
        GlVersion = "4.6"
    };
}
