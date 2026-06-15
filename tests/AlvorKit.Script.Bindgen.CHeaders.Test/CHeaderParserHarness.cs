namespace AlvorKit.Script.Bindgen.CHeaders.Test;

internal static class CHeaderParserHarness
{
    public static string WriteHeader(TempWorkspace workspace, string source, string contents)
    {
        var header = Path.Combine(source, "fixture.h");
        var translationUnit = Path.Combine(workspace.Root, "fixture.c");
        File.WriteAllText(header, contents);
        File.WriteAllText(translationUnit, """#include "source/fixture.h" """);
        return translationUnit;
    }

    public static BindingModel Parse(string translationUnit, string source) =>
        Parse(translationUnit, source, CHeaderTestConfig.Create());

    public static BindingModel Parse(string translationUnit, string source, BindgenConfig config) =>
        new CHeaderBindingParser(config, config.ApiClass).Parse(
            translationUnit,
            includeDirectory: source,
            filterRoot: source,
            libraryDirectory: source,
            targetTriple: "x86_64-pc-windows-msvc");
}
