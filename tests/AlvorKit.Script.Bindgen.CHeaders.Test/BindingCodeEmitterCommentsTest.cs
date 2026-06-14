using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterCommentsTest
{
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

        StringAssert.Contains(api, "/// <summary>Native constant value for <c>Answer</c>.</summary>");
        StringAssert.Contains(api, "/// <param name=\"left\">Native <c>left</c> parameter.</param>");
        StringAssert.Contains(value, "/// <summary>Maps <c>test_value</c>.</summary>");
        StringAssert.Contains(point, "/// <summary>Maps the native field at byte offset 0.</summary>");
        StringAssert.Contains(point, "/// <summary>First element storage used by the compiler-expanded inline array.</summary>");
        StringAssert.Contains(native, "/// <summary>Name of the native shared library.</summary>");
        StringAssert.Contains(backend, "/// <inheritdoc/>");
    }

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

    private static BindingModel ModelWithTypes() => new(
        Enums: [new("test_value", "TestValue", "int", false, [new("A", 1, null)], null)],
        Structs: [new("test_point", "TestPoint", false, 16, [new("X", "int", 0, null)], [new("ValuesBuffer", "int", 4)], null)],
        Handles: [new("test_handle", "TestHandle")],
        Delegates: [new("TestCallback", "void", [new("value", "int", "int", "", false)])],
        Functions:
        [
            new("test_add", "Add", "int", "int", [new("left", "int", "int", "", false)], null)
        ],
        Constants: [new("Answer", 42)],
        SkippedFunctions: [],
        SizeofTypes: []);
}
