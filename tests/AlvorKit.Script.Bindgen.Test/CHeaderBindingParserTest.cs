using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Test;

[TestClass]
public sealed class CHeaderBindingParserTest
{
    [TestMethod]
    public void Parse_IgnoresDeclarationsFromSiblingDirectoriesWithSamePrefix()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var sibling = workspace.CreateDirectory("source-other");
        var header = Path.Combine(source, "fixture.h");
        var siblingHeader = Path.Combine(sibling, "fixture_sibling.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");

        File.WriteAllText(header, """
            #define test_VISIBLE_CONSTANT 7
            int test_visible(void);
            const char* test_const_string(void);
            char* test_mutable_string(void);
            """);
        File.WriteAllText(siblingHeader, """
            #define test_HIDDEN_CONSTANT 9
            int test_hidden(void);
            """);
        File.WriteAllText(translationUnit, """
            #include "source/fixture.h"
            #include "source-other/fixture_sibling.h"
            """);

        var model = Parse(translationUnit, source);

        CollectionAssert.Contains(model.Functions.Select(function => function.NativeName).ToList(), "test_visible");
        CollectionAssert.DoesNotContain(model.Functions.Select(function => function.NativeName).ToList(), "test_hidden");
        CollectionAssert.Contains(model.Constants.Select(constant => constant.ManagedName).ToList(), "VisibleConstant");
        CollectionAssert.DoesNotContain(model.Constants.Select(constant => constant.ManagedName).ToList(), "HiddenConstant");
    }

    [TestMethod]
    public void Parse_OnlyConstCharPointerReturnsAreCStringConveniences()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var header = Path.Combine(source, "fixture.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");

        File.WriteAllText(header, """
            const char* test_const_string(void);
            char* test_mutable_string(void);
            """);
        File.WriteAllText(translationUnit, """#include "source/fixture.h" """);

        var model = Parse(translationUnit, source);

        Assert.IsTrue(model.Functions.Single(function => function.NativeName == "test_const_string").ReturnsCString);
        Assert.IsFalse(model.Functions.Single(function => function.NativeName == "test_mutable_string").ReturnsCString);
    }

    private static BindingModel Parse(string translationUnit, string source)
    {
        var config = TestConfig();
        return new CHeaderBindingParser(config, config.ApiClass).Parse(
            translationUnit,
            includeDirectory: source,
            filterRoot: source,
            libraryDirectory: source,
            targetTriple: "x86_64-pc-windows-msvc");
    }

    private static BindgenConfig TestConfig() => new()
    {
        Namespace = "AlvorKit.Bindgen.Fixture",
        ApiClass = "Test",
        ApiSummary = "Fixture API.",
        BackendClass = "TestBackend",
        NativeClass = "TestNative",
        NativeLibrary = "test",
        Prefix = "test_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };
}
