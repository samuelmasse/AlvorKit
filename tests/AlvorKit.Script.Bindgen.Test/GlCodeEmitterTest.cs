using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Test;

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

        new GlCodeEmitter(config, "registry-tag", "doc-tag").Emit(model, workspace.Root, "4.6.2");

        var delegateSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "GlDebugProc.cs"));
        StringAssert.Contains(delegateSource, "[UnmanagedFunctionPointer(CallingConvention.Winapi)]");
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
