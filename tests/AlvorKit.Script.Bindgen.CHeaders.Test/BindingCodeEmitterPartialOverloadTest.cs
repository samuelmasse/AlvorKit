namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingCodeEmitterPartialOverloadTest
{
    /// <summary>Typed convenience overloads are emitted as members on the generated partial API class.</summary>
    [TestMethod]
    public void Emit_TypedConvenienceOverloadsArePartialMembers()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.EnumOverloads = new()
        {
            ByParamName = { ["mode"] = "TestMode" },
            Functions =
            {
                ["test_run"] = new() { Return = "TestResult" },
                ["test_toggle"] = new() { Return = "TestResult", Params = { ["enabled"] = ["bool", "bool", "int"] } }
            }
        };
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new("test_run", "Run", "int", "int", [new("mode", "int", "int", "", false)], null),
                new("test_toggle", "Toggle", "int", "int", [new("enabled", "int", "int", "", false)], null),
                new("test_try_run", "TryRun", "int", "int",
                    [
                        new("mode", "int", "int", "", false),
                        new("seed", "int", "int", "", false),
                        new("count", "int", "int", "out", false)
                    ],
                    null),
                new("test_set_name", "SetName", "void", "void", [new("name", "nint", "nint", "", true)], null),
                new("test_open", "Open", "int", "int", [new("path", "nint", "nint", "", true)], null)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "using System.Text;");
        StringAssert.Contains(overloads, "public unsafe partial class Test");
        Assert.IsFalse(overloads.Contains("static class", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("this Test", StringComparison.Ordinal));
        StringAssert.Contains(overloads, "public TestResult Run(TestMode mode) => (TestResult)Run((int)mode);");
        StringAssert.Contains(overloads, "public TestResult Toggle(bool enabled) => (TestResult)Toggle((enabled ? 1 : 0));");
        StringAssert.Contains(
            overloads,
            "public int TryRun(TestMode mode, int seed, out int count) => TryRun((int)mode, seed, out count);");
        StringAssert.Contains(overloads, "using var nameUtf8 = new Utf8(name, stackalloc byte[256]);");
        StringAssert.Contains(overloads, "SetName(nameUtf8.Pointer);");
        StringAssert.Contains(overloads, "return Open(pathUtf8.Pointer);");
        StringAssert.Contains(overloads, "private readonly unsafe ref struct Utf8");
    }

    /// <summary>Span-return convenience overloads are emitted as members on the generated partial API class.</summary>
    [TestMethod]
    public void Emit_SpanReturnOverloadsArePartialMembers()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.SpanReturns = new() { ["test_items"] = "int" };
        var model = new BindingModel(
            [],
            [],
            [],
            [],
            [
                new(
                    "test_items",
                    "Items",
                    "nint",
                    "nint",
                    [new("source", "nint", "nint", "", false), new("count", "int", "int", "out", false)],
                    null)
            ],
            [],
            [],
            []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "TestOverloads.cs"));
        StringAssert.Contains(overloads, "public unsafe partial class Test");
        StringAssert.Contains(overloads, "public unsafe ReadOnlySpan<int> Items(nint source)");
        StringAssert.Contains(overloads, "var pointer = Items(source, out var count);");
        StringAssert.Contains(overloads, "return pointer == 0 || count <= 0 ? default : new ReadOnlySpan<int>((void*)pointer, count);");
    }

}
