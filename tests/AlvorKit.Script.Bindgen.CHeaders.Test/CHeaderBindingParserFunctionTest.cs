using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class CHeaderBindingParserFunctionTest
{
    [TestMethod]
    public void Parse_SkipsConfiguredAndVariadicFunctions()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            void test_skip(void);
            void test_log(const char* format, ...);
            static void test_static(void);
            void test_keep(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.Skip = new() { ["test_skip"] = "manual" };

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        CollectionAssert.AreEqual(new[] { "test_keep" }, model.Functions.Select(function => function.NativeName).ToArray());
        CollectionAssert.Contains(model.SkippedFunctions, "test_skip (manual)");
        CollectionAssert.Contains(model.SkippedFunctions, "test_log (variadic)");
    }

    [TestMethod]
    public void Parse_DetectsUntypedPointerSpanCandidates()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            #include <stddef.h>
            void test_write(void* data, size_t dataSize);
            void test_read(const void* data, size_t dataSize);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);
        var write = model.Functions.Single(function => function.NativeName == "test_write");
        var read = model.Functions.Single(function => function.NativeName == "test_read");

        Assert.IsTrue(write.Parameters[0].IsUntypedPointer);
        Assert.IsFalse(write.Parameters[0].IsConstPointee);
        Assert.IsTrue(write.Parameters[1].IsSizeT);
        Assert.IsTrue(read.Parameters[0].IsConstPointee);
    }

    [TestMethod]
    public void Parse_TracksSizeofCandidateForStructInitFunction()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_context { int value; } test_context;
            void test_context_init(test_context* context);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source);

        CollectionAssert.Contains(model.SizeofTypes, "test_context");
    }
}
