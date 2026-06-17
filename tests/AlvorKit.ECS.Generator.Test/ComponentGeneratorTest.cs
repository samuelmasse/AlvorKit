namespace AlvorKit.ECS.Generator.Test;

/// <summary>Tests source generation for ECS component interfaces.</summary>
[TestClass]
public sealed class ComponentGeneratorTest
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);

    /// <summary>Generated output includes component marker classes, read accessors, mutating accessors, and builders.</summary>
    [TestMethod]
    public void Generate_WithNamedComponents_EmitsAccessorsAndBuilder()
    {
        var result = Generate(
            """
            namespace Fixture;

            using AlvorKit.ECS.Generator;

            [Components]
            public interface IActorComponents
            {
                /// <summary>Current health.</summary>
                int Health { get; set; }
                string? Name { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        StringAssert.Contains(source, "namespace Fixture;");
        StringAssert.Contains(source, "public abstract class ActorComponents : IComponentGroup");
        StringAssert.Contains(source, "public bool HasHealth => ent.Has<int, ActorComponents.Health>();");
        StringAssert.Contains(source, "public int Health");
        StringAssert.Contains(source, "public EntMutator<T> Health(in int value)");
        StringAssert.Contains(source, "/// <summary>Current health.</summary>");
        AssertCompiles(result);
    }

    /// <summary>Generated output handles nullable values, delegate names, lazy initialization, and string output markers.</summary>
    [TestMethod]
    public void Generate_WithNullableDelegateLazyAndToString_EmitsExpectedShapes()
    {
        var result = Generate(
            """
            namespace Fixture;

            using System;
            using System.Collections.Generic;
            using AlvorKit.ECS.Generator;

            [Components]
            internal interface IAdvancedComponents
            {
                [ComponentLazyInitialize] List<int>? Inventory { get; set; }
                Action Tick { get; set; }
                [ComponentToString] int Score { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        StringAssert.Contains(source, "internal abstract class AdvancedComponents : IComponentGroup");
        StringAssert.Contains(source, "public System.Action TickDelegate");
        StringAssert.Contains(source, "var value = ent.Get<System.Collections.Generic.List<int>?, AdvancedComponents.Inventory>();");
        StringAssert.Contains(source, "value = new();");
        StringAssert.Contains(source, "[ComponentToString]");
        AssertCompiles(result);
    }

    /// <summary>SkipBuilder suppresses builder-style mutator extensions while keeping normal accessors.</summary>
    [TestMethod]
    public void Generate_WithSkipBuilder_OmitsBuilderExtension()
    {
        var result = Generate(
            """
            namespace Fixture;

            using AlvorKit.ECS.Generator;

            [Components(SkipBuilder = true)]
            public interface IPlainComponents
            {
                int Count { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        StringAssert.Contains(source, "public int Count");
        Assert.IsFalse(source.Contains("extension<T>(EntMutator<T> mut)", StringComparison.Ordinal));
        AssertCompiles(result);
    }

    /// <summary>Explicit false builder options keep builder-style mutator extensions.</summary>
    [TestMethod]
    public void Generate_WithSkipBuilderFalse_EmitsBuilderExtension()
    {
        var result = Generate(
            """
            namespace Fixture;

            using AlvorKit.ECS.Generator;

            [Components(SkipBuilder = false)]
            public interface IBuilderComponents
            {
                int Count { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        StringAssert.Contains(source, "extension<T>(EntMutator<T> mut)");
        StringAssert.Contains(source, "public EntMutator<T> Count(in int value)");
        AssertCompiles(result);
    }

    /// <summary>Empty component interfaces intentionally do not generate source.</summary>
    [TestMethod]
    public void Generate_WithEmptyInterface_EmitsNoSource()
    {
        var result = Generate(
            """
            using AlvorKit.ECS.Generator;

            [Components]
            public interface IEmptyComponents
            {
            }
            """);

        Assert.AreEqual(0, result.GeneratedTrees.Length);
        AssertCompiles(result);
    }

    /// <summary>Global-namespace interfaces generate source without a namespace declaration.</summary>
    [TestMethod]
    public void Generate_WithGlobalNamespace_OmitsNamespaceDeclaration()
    {
        var result = Generate(
            """
            using AlvorKit.ECS.Generator;

            [Components]
            public interface IGlobalComponents
            {
                int Value { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        Assert.IsTrue(source.StartsWith("// <auto-generated/>\nusing AlvorKit.ECS;", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("namespace ", StringComparison.Ordinal));
        AssertCompiles(result);
    }

    /// <summary>Non-component interface members are ignored while valid component properties still generate source.</summary>
    [TestMethod]
    public void Generate_WithUnsupportedMembers_IgnoresUnsupportedProperties()
    {
        var result = Generate(
            """
            namespace Fixture;

            using AlvorKit.ECS.Generator;

            [Components]
            public interface IFilteredComponents
            {
                int Count { get; set; }
                int ReadOnly { get; }
                static int StaticValue { get; set; }
                int this[int index] { get; set; }
            }
            """);

        var source = AssertSingleSource(result);
        StringAssert.Contains(source, "public abstract class Count : IComponent");
        Assert.IsFalse(source.Contains("ReadOnly", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("StaticValue", StringComparison.Ordinal));
        AssertCompiles(result);
    }

    /// <summary>Component access helpers map all generated access cases.</summary>
    [TestMethod]
    public void AccessHelpers_MapAccessibilitiesAndWidenAccess()
    {
        Assert.AreEqual("internal", ComponentAccess.ToAccessString(Accessibility.Internal));
        Assert.AreEqual("protected", ComponentAccess.ToAccessString(Accessibility.Protected));
        Assert.AreEqual("protected internal", ComponentAccess.ToAccessString(Accessibility.ProtectedOrInternal));
        Assert.AreEqual("private", ComponentAccess.ToAccessString(Accessibility.Private));
        Assert.AreEqual("public", ComponentAccess.ToAccessString(Accessibility.Public));
        Assert.AreEqual("public", ComponentAccess.ToAccessString(Accessibility.NotApplicable));

        Assert.AreEqual("private", ComponentAccess.WiderAccess("private", "private"));
        Assert.AreEqual("protected internal", ComponentAccess.WiderAccess("protected", "internal"));
        Assert.AreEqual("protected internal", ComponentAccess.WiderAccess("internal", "protected"));
        Assert.AreEqual("public", ComponentAccess.WiderAccess("public", "private"));
        Assert.AreEqual("protected internal", ComponentAccess.WiderAccess("protected internal", "internal"));
        Assert.AreEqual("public", ComponentAccess.WiderAccess("internal", "public"));
        Assert.AreEqual("public", ComponentAccess.WiderAccess("unknown", "public"));
    }

    /// <summary>Naming helpers handle non-interface names and ordinary non-delegate properties.</summary>
    [TestMethod]
    public void NamingHelpers_HandleNonInterfaceAndNonDelegateNames()
    {
        var property = new PropertyModel(
            Name: "Value",
            ValueType: "int",
            NullableType: "int",
            AddToString: false,
            LazyInitialize: false,
            IsDelegate: false,
            Comment: null,
            GetAccess: "public",
            SetAccess: "public");

        Assert.AreEqual("Plain", ComponentNames.StripInterfacePrefix("Plain"));
        Assert.AreEqual("Actor", ComponentNames.StripInterfacePrefix("IActor"));
        Assert.AreEqual("Value", ComponentNames.AccessorName(property));
    }

    /// <summary>Metadata-only symbols without source syntax do not contribute copied XML comments.</summary>
    [TestMethod]
    public void ModelFactory_WithMetadataSymbolWithoutSourceSyntax_ReturnsNoComment()
    {
        var compilation = CSharpCompilation.Create(
            "MetadataFixture",
            references: References(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var property = compilation.GetSpecialType(SpecialType.System_String)
            .GetMembers("Length")
            .OfType<IPropertySymbol>()
            .Single();
        var readXmlComment = typeof(ComponentModelFactory).GetMethod(
            "ReadXmlComment",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.IsNotNull(readXmlComment);
        Assert.IsNull(readXmlComment.Invoke(null, [property, CancellationToken.None]));
    }

    /// <summary>Template rendering fails loudly for missing placeholder values and missing embedded templates.</summary>
    [TestMethod]
    public void TemplateRendering_WhenInvalid_Throws()
    {
        var missingPlaceholder = Assert.ThrowsExactly<InvalidOperationException>(
            () => ComponentTemplate.Render("has-property.csfrag.tmpl"));
        StringAssert.Contains(missingPlaceholder.Message, "{{Comment}}");

        var missingTemplate = Assert.ThrowsExactly<FileNotFoundException>(
            () => ComponentTemplate.Render("missing.tmpl"));
        StringAssert.Contains(missingTemplate.Message, "missing.tmpl");
    }

    /// <summary>Runs the component generator against a source snippet.</summary>
    private static GeneratorResult Generate(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), ParseOptions);
        var compilation = CSharpCompilation.Create(
            "GeneratorFixture",
            [syntaxTree],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        IIncrementalGenerator[] generators = [new ComponentGenerator()];
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions(ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        AssertNoErrors(diagnostics);
        var runResult = driver.GetRunResult().Results.Single();
        AssertNoErrors(runResult.Diagnostics);

        return new([.. runResult.GeneratedSources.Select(source => source.SourceText.ToString())], outputCompilation);
    }

    /// <summary>Returns the only generated source or fails when generation produced a different count.</summary>
    private static string AssertSingleSource(GeneratorResult result)
    {
        Assert.AreEqual(1, result.GeneratedTrees.Length);
        return result.GeneratedTrees[0];
    }

    /// <summary>Asserts that the generated compilation has no errors.</summary>
    private static void AssertCompiles(GeneratorResult result) =>
        AssertNoErrors(result.Compilation.GetDiagnostics());

    /// <summary>Asserts that a diagnostic collection contains no errors.</summary>
    private static void AssertNoErrors(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual("", string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
    }

    /// <summary>Returns platform and project references needed by generator fixture compilations.</summary>
    private static MetadataReference[] References()
    {
        var platformReferences = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        MetadataReference[] projectReferences =
        [
            MetadataReference.CreateFromFile(typeof(EntObj).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ComponentsAttribute).Assembly.Location)
        ];

        return [.. platformReferences.Concat(projectReferences)];
    }

    /// <summary>Stores generated source and its updated compilation.</summary>
    private sealed record GeneratorResult(string[] GeneratedTrees, Compilation Compilation);
}
