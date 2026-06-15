namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterUInt128InteropTest
{
    [TestMethod]
    public void EmitBackend_ConvertsUInt128InteropStruct()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.TypeAliases = new() { ["test_hash128"] = "UInt128" };
        config.InteropTypeAliases = new() { ["test_hash128"] = "TestHash128" };
        var hashStruct = new BindingStruct(
            NativeName: "test_hash128",
            ManagedName: "TestHash128",
            IsUnion: false,
            Size: 16,
            Fields:
            [
                new("Low64", "ulong", 0, null),
                new("High64", "ulong", 8, null)
            ],
            NestedBuffers: [],
            Documentation: null);
        var model = new BindingModel(
            Enums: [],
            Structs: [hashStruct],
            Handles: [],
            Delegates: [],
            Functions:
            [
                new(
                    NativeName: "test_hash_data",
                    ManagedName: "HashData",
                    ReturnType: "UInt128",
                    ReturnInteropType: "TestHash128",
                    Parameters: [new("seed", "UInt128", "TestHash128", "", HasStringConvenience: false)],
                    Documentation: null)
            ],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var structSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestHash128.cs"));
        var nativeSource = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestNative.cs"));
        var backendSource = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs"));

        StringAssert.Contains(structSource, "public readonly UInt128 ToUInt128()");
        StringAssert.Contains(structSource, "public static TestHash128 FromUInt128(UInt128 value)");
        StringAssert.Contains(nativeSource, "public static partial TestHash128 HashData(TestHash128 seed);");
        StringAssert.Contains(backendSource, "TestNative.HashData(TestHash128.FromUInt128(seed)).ToUInt128()");
    }

    [TestMethod]
    public void EmitBackend_ConvertsBoolAndCastArgumentsForNativeCalls()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = new BindingModel(
            Enums: [],
            Structs: [],
            Handles: [],
            Delegates: [],
            Functions:
            [
                new(
                    NativeName: "test_set_state",
                    ManagedName: "SetState",
                    ReturnType: "bool",
                    ReturnInteropType: "int",
                    Parameters:
                    [
                        new("enabled", "bool", "int", "", HasStringConvenience: false),
                        new("mode", "TestMode", "int", "", HasStringConvenience: false)
                    ],
                    Documentation: null)
            ],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var backendSource = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs"));
        StringAssert.Contains(backendSource, "TestNative.SetState(enabled ? (int)1 : (int)0, (int)mode) != 0");
    }
}
