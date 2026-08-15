namespace AlvorKit;

/// <summary>Verifies project-wide imports are preserved as LiveCode assembly metadata.</summary>
[TestClass]
public sealed class LiveCodeGlobalUsingGeneratorTest
{
    /// <summary>The generator captures normal, static, and aliased global usings while ignoring file imports.</summary>
    [TestMethod]
    public void Generate_WithProjectGlobalUsings_EmitsAssemblyMetadata()
    {
        const string source = """
            global using System;
            global using static System.Math;
            global using Text = System.String;
            using System.Text;

            namespace Fixture;

            public sealed class Marker;
            """;
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "LiveCodeGlobalUsingFixture",
            [CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions)],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new LiveCodeGlobalUsingGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var output,
            out var generatorDiagnostics);

        AssertNoErrors(generatorDiagnostics.Concat(output.GetDiagnostics()));
        var run = driver.GetRunResult();
        var generatedSources = run.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToArray();
        Assert.AreEqual(
            1,
            generatedSources.Length,
            string.Join(
                Environment.NewLine,
                run.Diagnostics
                    .Concat(run.Results.SelectMany(static result => result.Diagnostics))
                    .Select(static diagnostic => diagnostic.ToString())));
        var generated = generatedSources
            .Single()
            .SourceText
            .ToString()
            .ReplaceLineEndings("\n");
        StringAssert.Contains(
            generated,
            "[assembly: global::AlvorKit.LiveCodeGlobalUsingAttribute(\"System\")]");
        StringAssert.Contains(
            generated,
            "[assembly: global::AlvorKit.LiveCodeGlobalUsingAttribute(\"static System.Math\")]");
        StringAssert.Contains(
            generated,
            "[assembly: global::AlvorKit.LiveCodeGlobalUsingAttribute(\"Text = System.String\")]");
        Assert.IsFalse(generated.Contains("System.Text", StringComparison.Ordinal));
    }

    /// <summary>The template renderer reports missing resources and unresolved placeholders.</summary>
    [TestMethod]
    public void TemplateRendererRejectsInvalidRequests()
    {
        _ = Assert.ThrowsExactly<InvalidOperationException>(
            () => LiveCodeGeneratorTemplate.Render("global-usings.cs.tmpl"));
        _ = Assert.ThrowsExactly<FileNotFoundException>(
            () => LiveCodeGeneratorTemplate.Render("missing.tmpl"));
    }

    /// <summary>Returns platform and LiveCode references for the in-memory consumer compilation.</summary>
    private static IEnumerable<MetadataReference> References()
    {
        var platform = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path));
        return platform.Append(
            MetadataReference.CreateFromFile(
                typeof(LiveCodeGlobalUsingAttribute).Assembly.Location));
    }

    /// <summary>Fails with all Roslyn errors so generated-source problems remain readable.</summary>
    private static void AssertNoErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
            0,
            errors.Length,
            string.Join(
                Environment.NewLine,
                errors.Select(static error => error.ToString())));
    }
}
