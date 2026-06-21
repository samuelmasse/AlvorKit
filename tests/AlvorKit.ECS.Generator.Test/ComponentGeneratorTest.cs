namespace AlvorKit.ECS.Generator.Test;

/// <summary>Tests source emission for ECS component interface models.</summary>
[TestClass]
public sealed class ComponentGeneratorTest
{
    /// <summary>Generated output includes component marker classes, read accessors, mutating accessors, and builders.</summary>
    [TestMethod]
    public void Emit_WithNamedComponents_EmitsAccessorsAndBuilder()
    {
        var source = ComponentSourceEmitter.Emit(new(
            Namespace: "Fixture",
            InterfaceName: "IActorComponents",
            ClassName: "ActorComponents",
            Properties:
            [
                new(
                    Name: "Health",
                    ValueType: "int",
                    NullableType: "int",
                    AddToString: false,
                    LazyInitialize: false,
                    IsDelegate: false,
                    Comment: "/// <summary>Current health.</summary>",
                    GetAccess: "public",
                    SetAccess: "public"),
                new(
                    Name: "Name",
                    ValueType: "string",
                    NullableType: "string?",
                    AddToString: false,
                    LazyInitialize: false,
                    IsDelegate: false,
                    Comment: null,
                    GetAccess: "public",
                    SetAccess: "public")
            ],
            SkipBuilder: false,
            Access: "public"));

        StringAssert.Contains(source, "namespace Fixture;");
        StringAssert.Contains(source, "public abstract class ActorComponents : IComponentGroup");
        StringAssert.Contains(source, "public bool HasHealth => ent.Has<int, ActorComponents.Health>();");
        StringAssert.Contains(source, "public int Health");
        StringAssert.Contains(source, "public EntMutator<T> Health(in int value)");
        StringAssert.Contains(source, "/// <summary>Current health.</summary>");
    }

    /// <summary>Generated output handles nullable values, delegate names, lazy initialization, and string output markers.</summary>
    [TestMethod]
    public void Emit_WithNullableDelegateLazyAndToString_EmitsExpectedShapes()
    {
        var source = ComponentSourceEmitter.Emit(new(
            Namespace: "Fixture",
            InterfaceName: "IAdvancedComponents",
            ClassName: "AdvancedComponents",
            Properties:
            [
                new(
                    Name: "Inventory",
                    ValueType: "System.Collections.Generic.List<int>",
                    NullableType: "System.Collections.Generic.List<int>?",
                    AddToString: false,
                    LazyInitialize: true,
                    IsDelegate: false,
                    Comment: null,
                    GetAccess: "public",
                    SetAccess: "public"),
                new(
                    Name: "Tick",
                    ValueType: "System.Action",
                    NullableType: "System.Action",
                    AddToString: false,
                    LazyInitialize: false,
                    IsDelegate: true,
                    Comment: null,
                    GetAccess: "public",
                    SetAccess: "public"),
                new(
                    Name: "Score",
                    ValueType: "int",
                    NullableType: "int",
                    AddToString: true,
                    LazyInitialize: false,
                    IsDelegate: false,
                    Comment: null,
                    GetAccess: "public",
                    SetAccess: "public")
            ],
            SkipBuilder: false,
            Access: "internal"));

        StringAssert.Contains(source, "internal abstract class AdvancedComponents : IComponentGroup");
        StringAssert.Contains(source, "public System.Action TickDelegate");
        StringAssert.Contains(source, "var value = ent.Get<System.Collections.Generic.List<int>?, AdvancedComponents.Inventory>();");
        StringAssert.Contains(source, "value = new();");
        StringAssert.Contains(source, "[ComponentToString]");
    }

    /// <summary>SkipBuilder suppresses builder-style mutator extensions while keeping normal accessors.</summary>
    [TestMethod]
    public void Emit_WithSkipBuilder_OmitsBuilderExtension()
    {
        var source = ComponentSourceEmitter.Emit(PlainModel(skipBuilder: true));

        StringAssert.Contains(source, "public int Count");
        Assert.IsFalse(source.Contains("extension<T>(EntMutator<T> mut)", StringComparison.Ordinal));
    }

    /// <summary>Explicit false builder options keep builder-style mutator extensions.</summary>
    [TestMethod]
    public void Emit_WithSkipBuilderFalse_EmitsBuilderExtension()
    {
        var source = ComponentSourceEmitter.Emit(PlainModel(skipBuilder: false));

        StringAssert.Contains(source, "extension<T>(EntMutator<T> mut)");
        StringAssert.Contains(source, "public EntMutator<T> Count(in int value)");
    }

    /// <summary>Global-namespace interfaces generate source without a namespace declaration.</summary>
    [TestMethod]
    public void Emit_WithGlobalNamespace_OmitsNamespaceDeclaration()
    {
        var source = ComponentSourceEmitter.Emit(PlainModel(@namespace: ""));
        var normalizedSource = source.ReplaceLineEndings("\n");

        Assert.IsTrue(normalizedSource.StartsWith("// <auto-generated/>\nusing AlvorKit.ECS;", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("namespace ", StringComparison.Ordinal));
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

    /// <summary>Fragment rendering trims template padding and returns one trailing blank line.</summary>
    [TestMethod]
    public void TemplateRendering_ForFragment_NormalizesTrailingNewlines()
    {
        var output = ComponentTemplate.RenderFragment(
            "builder-method.csfrag.tmpl",
            ("Comment", ""),
            ("SetAccess", "public"),
            ("GetAccess", "public"),
            ("Name", "Health"),
            ("AccessorName", "Health"),
            ("Type", "int"));

        Assert.IsTrue(output.EndsWith("\n\n", StringComparison.Ordinal));
        Assert.IsFalse(output.EndsWith("\n\n\n", StringComparison.Ordinal));
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

    /// <summary>Builds a simple component interface model used by emitter tests.</summary>
    private static InterfaceModel PlainModel(bool skipBuilder = false, string @namespace = "Fixture") =>
        new(
            Namespace: @namespace,
            InterfaceName: "IPlainComponents",
            ClassName: "PlainComponents",
            Properties:
            [
                new(
                    Name: "Count",
                    ValueType: "int",
                    NullableType: "int",
                    AddToString: false,
                    LazyInitialize: false,
                    IsDelegate: false,
                    Comment: null,
                    GetAccess: "public",
                    SetAccess: "public")
            ],
            SkipBuilder: skipBuilder,
            Access: "public");
}
