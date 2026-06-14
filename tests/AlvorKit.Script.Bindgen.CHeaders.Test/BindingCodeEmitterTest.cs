using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

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

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var nativeSource = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestNative.cs"));
        StringAssert.Contains(nativeSource, "[LibraryImport(Lib, EntryPoint = \"test_add\")]");
        StringAssert.Contains(nativeSource, "[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]");
    }

    [TestMethod]
    public void EmitBackendProject_AlwaysReferencesNativePackage()
    {
        using var workspace = TempWorkspace.Create();
        var config = TestConfig();
        var model = new BindingModel([], [], [], [], [], [], [], []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "2.0.0", "1.0.0");

        var backendProject = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "Fixture.Backend.csproj"));
        StringAssert.Contains(backendProject, "<Version>2.0.0</Version>");
        StringAssert.Contains(backendProject, "<PackageReference Include=\"AlvorKit.Bindgen.Fixture.Native\" Version=\"1.0.0\" />");
        Assert.IsFalse(backendProject.Contains("AlvorKitSkipNativePackageReference", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EmitStringReturn_SpanOverloadHandlesNullNativePointer()
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
                    NativeName: "test_description",
                    ManagedName: "Description",
                    ReturnType: "nint",
                    ReturnInteropType: "nint",
                    Parameters: [],
                    Documentation: null,
                    ReturnsCString: true)
            ],
            Constants: [],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var apiSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(apiSource, "var pointer = Description();");
        StringAssert.Contains(apiSource, "if (pointer == 0) { result = default; return; }");
        StringAssert.Contains(apiSource, "MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)pointer)");
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
