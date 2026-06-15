using System.Text.RegularExpressions;

namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Structural tests for the OpenGL registry bindgen source project.</summary>
[TestClass]
public sealed class OpenGlRegistryStructureTest
{
    /// <summary>Every source file in the target project stays under the requested size limit.</summary>
    [TestMethod]
    public void SourceFiles_AreAtMost150Lines()
    {
        foreach (var file in SourceFiles())
        {
            var lines = File.ReadAllLines(file).Length;
            Assert.IsTrue(lines <= 150, $"{Path.GetFileName(file)} has {lines} lines.");
        }
    }

    /// <summary>Every source file in the target project declares exactly one top-level type.</summary>
    [TestMethod]
    public void SourceFiles_DeclareOneTopLevelType()
    {
        const string typeDeclarationPattern =
            @"^\s*(?:public|internal|private|file)\s+"
            + @"(?:(?:sealed|static|readonly|partial)\s+)*"
            + @"(?:class|record(?:\s+struct)?|struct|interface|enum)\s+\w+";
        var pattern = new Regex(typeDeclarationPattern, RegexOptions.Multiline);
        foreach (var file in SourceFiles())
        {
            var count = pattern.Matches(File.ReadAllText(file)).Count;
            Assert.AreEqual(1, count, Path.GetFileName(file));
        }
    }

    /// <summary>Every source file in the target project stays at the root or one subdirectory below it.</summary>
    [TestMethod]
    public void SourceFiles_UseOnlyOneSubdirectoryLevel()
    {
        var sourceRoot = SourceRoot();
        foreach (var file in SourceFiles())
        {
            var relativePath = Path.GetRelativePath(sourceRoot, file);
            var parts = relativePath.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(parts.Length <= 2, $"{relativePath} is nested too deeply.");
        }
    }

    /// <summary>Returns source files from the OpenGL registry bindgen project.</summary>
    private static IEnumerable<string> SourceFiles()
        => Directory.EnumerateFiles(SourceRoot(), "*.cs", SearchOption.AllDirectories);

    /// <summary>Returns the OpenGL registry bindgen source root.</summary>
    private static string SourceRoot()
        => Path.Combine(RepositoryRoot(), "scripts", "AlvorKit.Script.Bindgen.OpenGLRegistry");

    /// <summary>Finds the repository root from the test output directory.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "scripts", "AlvorKit.Script.Bindgen.OpenGLRegistry")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not locate repository root.");
    }
}
