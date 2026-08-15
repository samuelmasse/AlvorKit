namespace AlvorKit;

/// <summary>Enforces the hand-authored source shape of the native profiler.</summary>
[TestClass]
public sealed class NativeProfilerSourceShapeAuditTest
{
    private const int MaximumSourceLines = 250;
    private const int MaximumLineLength = 170;

    /// <summary>Internal native implementation and header files stay within the source-size target.</summary>
    [TestMethod]
    public void InternalSourceFiles_StayWithinLineTarget()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoot = Path.Combine(
            repositoryRoot,
            "native",
            "interception-profiler",
            "src");
        var violations = Directory
            .EnumerateFiles(sourceRoot, "*", SearchOption.TopDirectoryOnly)
            .Where(IsNativeSource)
            .Select(
                path =>
                    new
                    {
                        Path = Relative(repositoryRoot, path),
                        Lines = File.ReadLines(path).Count()
                    })
            .Where(file => file.Lines > MaximumSourceLines)
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .Select(file => $"{file.Path}: {file.Lines} lines")
            .ToArray();

        Assert.HasCount(
            0,
            violations,
            $"Native profiler source-size violations: " +
            $"{string.Join(", ", violations)}.");
    }

    /// <summary>Hand-authored profiler source, public ABI, and CMake files respect the line-length limit.</summary>
    [TestMethod]
    public void HandAuthoredFiles_StayWithinLineLengthLimit()
    {
        var repositoryRoot = FindRepositoryRoot();
        var profilerRoot = Path.Combine(
            repositoryRoot,
            "native",
            "interception-profiler");
        var files = Directory
            .EnumerateFiles(
                Path.Combine(profilerRoot, "src"),
                "*",
                SearchOption.TopDirectoryOnly)
            .Where(IsNativeSource)
            .Append(
                Path.Combine(
                    profilerRoot,
                    "include",
                    "alvorkit_interception_profiler.h"))
            .Append(Path.Combine(profilerRoot, "CMakeLists.txt"));
        var violations = files
            .SelectMany(
                path => File.ReadLines(path)
                    .Select(
                        (line, index) =>
                            new
                            {
                                Path = Relative(repositoryRoot, path),
                                Line = index + 1,
                                line.Length
                            }))
            .Where(line => line.Length > MaximumLineLength)
            .OrderBy(line => line.Path, StringComparer.Ordinal)
            .ThenBy(line => line.Line)
            .Select(
                line =>
                    $"{line.Path}:{line.Line}: {line.Length} characters")
            .ToArray();

        Assert.HasCount(
            0,
            violations,
            $"Native profiler line-length violations: " +
            $"{string.Join(", ", violations)}.");
    }

    /// <summary>The CMake target discovers every top-level profiler implementation and reconfigures when the set changes.</summary>
    [TestMethod]
    public void ImplementationFiles_AreDiscoveredByCMakeTarget()
    {
        var repositoryRoot = FindRepositoryRoot();
        var profilerRoot = Path.Combine(
            repositoryRoot,
            "native",
            "interception-profiler");
        var cmake = File.ReadAllText(
            Path.Combine(profilerRoot, "CMakeLists.txt"));

        StringAssert.Contains(
            cmake,
            "file(GLOB ALVORKIT_PROFILER_SOURCES CONFIGURE_DEPENDS");
        StringAssert.Contains(
            cmake,
            "\"${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp\"");
    }

    private static bool IsNativeSource(string path) =>
        Path.GetExtension(path) is ".cpp" or ".hpp" or ".c" or ".h";

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find the AlvorKit repository root.");
    }

    private static string Relative(string repositoryRoot, string path) =>
        Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');
}
