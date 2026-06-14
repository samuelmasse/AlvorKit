namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Creates minimal temporary repositories for native build tests.</summary>
internal static class TestRepositoryFactory
{
    /// <summary>Creates a repository with one native library manifest.</summary>
    public static string CreateSingleCLibrary(string name, string workDir, string revision = "2")
    {
        var root = CreateRoot();
        var library = Path.Combine(root, "native", name);
        Directory.CreateDirectory(library);
        File.WriteAllText(Path.Combine(root, "AlvorKit.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(library, "TAG"), "1.2.3");
        File.WriteAllText(Path.Combine(library, "REVISION"), revision);
        File.WriteAllText(Path.Combine(library, "shim.c"), "int test(void) { return 0; }");
        File.WriteAllText(Path.Combine(library, "bindgen.json"), $$"""
            {
                "nativeLibrary": "{{name}}",
                "workDir": "{{workDir}}",
                "sourceDir": "src-{tag}",
                "sourceUrl": "https://example.invalid/{{name}}.tar.gz",
                "implFile": "shim.c"
            }
            """);
        File.WriteAllText(Path.Combine(library, "native-build.json"), """
            {
                "kind": "single-c",
                "linux": {
                    "packages": ["build-essential"],
                    "linkLibraries": ["m"],
                    "allowedDependencies": ["libc.so.6", "libm.so.6"]
                }
            }
            """);
        return root;
    }

    /// <summary>Creates a repository with one CMake native library manifest.</summary>
    public static string CreateCMakeLibrary(string name, string workDir)
    {
        var root = CreateSingleCLibrary(name, workDir, revision: "");
        var library = Path.Combine(root, "native", name);
        File.WriteAllText(Path.Combine(library, "native-build.json"), """
            {
                "kind": "cmake",
                "linux": {
                    "packages": ["build-essential", "cmake"],
                    "cmakeOutput": "src/libsample.so",
                    "cmakeOptions": ["-DBUILD_SHARED_LIBS=ON"],
                    "allowedDependencies": ["libc.so.6"]
                },
                "macos": {
                    "cmakeOutput": "src/libsample.dylib",
                    "cmakeOptions": ["-DBUILD_SHARED_LIBS=ON"]
                }
            }
            """);
        return root;
    }

    /// <summary>Creates an empty repository root in the temp directory.</summary>
    private static string CreateRoot() =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "alvorkit-native-test-" + Guid.NewGuid().ToString("N"))).FullName;
}
