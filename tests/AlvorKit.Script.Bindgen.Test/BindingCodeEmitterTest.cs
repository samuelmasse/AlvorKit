using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Test;

[TestClass]
public sealed class BindingCodeEmitterTest
{
    [TestMethod]
    public void EmitNativeImports_UsesExplicitCdeclCallingConvention()
    {
        using var workspace = TempWorkspace.Create();
        var config = TestConfig();
        var model = new BindingModel(
            Enums: [],
            Structs: [],
            Handles: [],
            Delegates: [],
            Functions:
            [
                new(
                    NativeName: "test_add",
                    ManagedName: "Add",
                    ReturnType: "int",
                    ReturnInteropType: "int",
                    Parameters:
                    [
                        new("left", "int", "int", "", HasStringConvenience: false),
                        new("right", "int", "int", "", HasStringConvenience: false)
                    ],
                    Documentation: null)
            ],
            Constants: [],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0");

        var nativeSource = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestNative.cs"));
        StringAssert.Contains(nativeSource, "[LibraryImport(Lib, EntryPoint = \"test_add\")]");
        StringAssert.Contains(nativeSource, "[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
    }

    [TestMethod]
    public void EmitBackendProject_CanSkipNativePackageReferenceForCompileTests()
    {
        using var workspace = TempWorkspace.Create();
        var config = TestConfig();
        var model = new BindingModel([], [], [], [], [], [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0");

        var backendProject = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "Fixture.Backend.csproj"));
        StringAssert.Contains(backendProject, "Condition=\"'$(AlvorKitSkipNativePackageReference)' != 'true'\"");
        StringAssert.Contains(backendProject, "<PackageReference Include=\"AlvorKit.Bindgen.Fixture.Native\" Version=\"1.0.0\" />");
    }

    private static BindgenConfig TestConfig() => new()
    {
        Namespace = "AlvorKit.Bindgen.Fixture",
        ApiClass = "Test",
        ApiSummary = "Fixture API.",
        BackendClass = "TestBackend",
        NativeClass = "TestNative",
        NativeLibrary = "fixture",
        Prefix = "test_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };
}
