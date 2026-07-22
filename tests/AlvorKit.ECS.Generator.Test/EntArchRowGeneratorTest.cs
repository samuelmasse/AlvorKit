namespace AlvorKit.ECS.Generator.Test;

/// <summary>Tests exact archetypal row source generation from consumer query usage.</summary>
[TestClass]
public sealed class EntArchRowGeneratorTest
{
    /// <summary>Named and raw query chains generate compiling exact rows only for the used shapes.</summary>
    [TestMethod]
    public void Generate_RowsCalls_EmitCompilingExactShapes()
    {
        const string source = """
            namespace Fixture;

            using AlvorKit.ECS;
            using AlvorKit.ECS.Generator;

            [Components]
            public interface IMotionComponents
            {
                [Archetypal] Position Position { get; set; }
                [Archetypal] Velocity Velocity { get; set; }
                int Sparse { get; set; }
            }

            public struct Position { public int X; }
            public struct Velocity { public int X; }
            internal readonly record struct RawArch;
            internal readonly record struct RawValue
            {
                internal const string EntArchGetAccess = "internal";
                internal const string EntArchSetAccess = "private";
            }

            public static class MotionSystem
            {
                public static int Run(EntArena arena)
                {
                    var moving = arena.QueryArchetypal<MotionComponents>()
                        .WithPosition()
                        .WithVelocity();
                    int sum = 0;
                    foreach (var row in moving.Rows())
                    {
                        row.Position.X += row.Velocity.X;
                        sum += row.Ent.IsAlive ? 1 : 0;
                    }

                    var reversed = arena.QueryArchetypal<MotionComponents>()
                        .WithVelocity()
                        .WithPosition();
                    foreach (var row in reversed.Rows())
                        sum += row.Position.X + row.Velocity.X;

                    var raw = arena.QueryArchetypal<RawArch>().With<int, RawValue>();
                    foreach (var row in raw.Rows())
                    {
                        row.RawValue++;
                        sum += row.RawValue;
                    }
                    return sum;
                }
            }
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RowFixture",
            [CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions)],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ComponentGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);

        AssertNoErrors(generatorDiagnostics.Concat(output.GetDiagnostics()));
        var generated = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(result => result.SourceText.ToString())
            .ToArray();
        Assert.AreEqual(4, generated.Length);
        Assert.AreEqual(3, generated.Count(text => text.Contains("static class EntArchRowsExtensions_", StringComparison.Ordinal)));
        Assert.IsTrue(generated.Any(text => text.Contains("ref Fixture.Position Position", StringComparison.Ordinal)));
        Assert.IsTrue(generated.Any(text => text.Contains("ref Fixture.Velocity Velocity", StringComparison.Ordinal)));
        Assert.IsTrue(generated.Any(text => text.Contains("internal ref int RawValue", StringComparison.Ordinal)));
        Assert.IsTrue(generated.Any(text => text.Contains("private readonly nint row;", StringComparison.Ordinal)));
        Assert.IsTrue(generated.Any(text => text.Contains("private nint count;", StringComparison.Ordinal)));
        Assert.IsFalse(generated.Any(text => text.Contains("MoveNextArch()", StringComparison.Ordinal)));
        Assert.IsFalse(generated.Any(text => text.Contains("ReadPosition()", StringComparison.Ordinal)));
        Assert.IsFalse(generated.Any(text => text.Contains("WritePosition()", StringComparison.Ordinal)));
        Assert.IsFalse(generated.Any(text => text.Contains("ref int Sparse", StringComparison.Ordinal)));
    }

    /// <summary>Rows fully qualify a generated component group declared outside the caller namespace.</summary>
    [TestMethod]
    public void Generate_RowsCallWithImportedGroup_EmitsQualifiedGroupType()
    {
        const string source = """
            namespace Fixture.Components
            {
                using AlvorKit.ECS.Generator;

                [Components]
                public interface ISeparatedComponents
                {
                    [Archetypal] int Value { get; set; }
                }
            }

            namespace Fixture.Systems
            {
                using AlvorKit.ECS;
                using Fixture.Components;

                public static class Runner
                {
                    public static void Run(EntArena arena)
                    {
                        var query = arena.QueryArchetypal<SeparatedComponents>().WithValue();
                        foreach (var row in query.Rows())
                            row.Value++;
                    }
                }
            }
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "SeparatedRowFixture",
            [CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions)],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ComponentGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);

        AssertNoErrors(generatorDiagnostics.Concat(output.GetDiagnostics()));
        var generated = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(result => result.SourceText.ToString())
            .ToArray();
        Assert.IsTrue(generated.Any(text => text.Contains(
            "global::Fixture.Components.SeparatedComponents",
            StringComparison.Ordinal)));
    }

    /// <summary>Returns platform and ECS references for the in-memory consumer compilation.</summary>
    private static IEnumerable<MetadataReference> References()
    {
        var platform = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        return platform.Append(MetadataReference.CreateFromFile(typeof(EntArena).Assembly.Location));
    }

    /// <summary>Fails with all Roslyn errors so a generated-source problem remains readable.</summary>
    private static void AssertNoErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
    }
}
