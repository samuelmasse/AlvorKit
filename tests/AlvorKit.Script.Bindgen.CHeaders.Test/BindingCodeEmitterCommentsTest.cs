namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterCommentsTest
{
    /// <summary>Generated members receive fallback XML docs when no native docs are available.</summary>
    [TestMethod]
    public void Emit_AddsFallbackXmlDocsToGeneratedMembers()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        var model = ModelWithTypes();

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        var value = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestValue.cs"));
        var point = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestPoint.cs"));
        var native = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestNative.cs"));
        var backend = File.ReadAllText(Path.Combine(workspace.Root, config.BackendProject, "TestBackend.cs"));

        StringAssert.Contains(api, "/// <param name=\"left\">Native <c>left</c> parameter.</param>");
        StringAssert.Contains(value, "/// <summary>Native enum <c>test_value</c>.</summary>");
        StringAssert.Contains(value, "/// <summary><c>test_VALUE_A</c>.</summary>");
        Assert.IsFalse(value.Contains("/// <summary><c>A</c>.</summary>", StringComparison.Ordinal));
        StringAssert.Contains(point, "/// <summary>Maps the native field at byte offset 0.</summary>");
        StringAssert.Contains(point, "/// <summary>First element storage used by the compiler-expanded inline array.</summary>");
        StringAssert.Contains(native, "/// <summary>Name of the native shared library.</summary>");
        StringAssert.Contains(backend, "/// <inheritdoc/>");
    }

    /// <summary>Generated standalone types are written as separate source files.</summary>
    [TestMethod]
    public void Emit_WritesEachGeneratedTypeToItsOwnFile()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();

        new BindingCodeEmitter(config, "1.0.0").Emit(ModelWithTypes(), workspace.Root, "1.0.0", "1.0.0");

        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, config.ApiProject, "TestValue.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, config.ApiProject, "TestPoint.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, config.ApiProject, "TestHandle.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, config.ApiProject, "TestCallback.cs")));
        Assert.IsFalse(File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs")).Contains("public struct TestPoint", StringComparison.Ordinal));
    }

    /// <summary>Boolean return documentation describes the public managed bool shape.</summary>
    [TestMethod]
    public void Emit_BoolReturnDocumentationMatchesManagedReturn()
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
                new("test_ready", "Ready", "bool", "int", [], Documentation: null)
            ],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var api = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "Test.cs"));
        StringAssert.Contains(api, "/// <returns>true when the native function returns non-zero; otherwise, false.</returns>");
    }

    private static BindingModel ModelWithTypes() => new(
        Enums: [new("test_value", "TestValue", "int", false, [new("test_VALUE_A", "A", 1, null)], null)],
        Structs: [new("test_point", "TestPoint", false, 16, [new("X", "int", 0, null)], [new("ValuesBuffer", "int", 4)], null)],
        Handles: [new("test_handle", "TestHandle")],
        Delegates: [new("TestCallback", "void", [new("value", "int", "int", "", false)])],
        Functions:
        [
            new("test_add", "Add", "int", "int", [new("left", "int", "int", "", false)], null)
        ],
        SkippedFunctions: [],
        SizeofTypes: []);
}
