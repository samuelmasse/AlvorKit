namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Tests for generated binding XML documentation text.</summary>
[TestClass]
public sealed class BindingDocsTest
{
    /// <summary>Function docs use native documentation, punctuation, return text, and remarks when available.</summary>
    [TestMethod]
    public void Function_WithDocumentation_EmitsProvidedText()
    {
        var output = new StringBuilder();
        var function = new BindingFunction(
            NativeName: "test_value",
            ManagedName: "Value",
            ReturnType: "int",
            ReturnInteropType: "int",
            Parameters: [new("input", "int", "int", "", HasStringConvenience: false)],
            Documentation: new("gets a value", new() { ["input"] = "Input value." }, "The value.", "Important."));

        BindingDocs.Function(output, function);

        var docs = output.ToString();
        StringAssert.Contains(docs, "<summary><c>test_value</c> - Gets a value.</summary>");
        StringAssert.Contains(docs, "<param name=\"input\"><c>input</c> - Input value.</param>");
        StringAssert.Contains(docs, "<returns>Return value from <c>test_value</c>: The value.</returns>");
        StringAssert.Contains(docs, "<remarks><c>test_value</c>: Important.</remarks>");
    }

    /// <summary>Function docs fall back for missing native text and describe bool interop projections.</summary>
    [TestMethod]
    public void Function_MissingDocumentation_EmitsFallbacks()
    {
        var output = new StringBuilder();
        var function = new BindingFunction(
            NativeName: "test_flag",
            ManagedName: "Flag",
            ReturnType: "bool",
            ReturnInteropType: "byte",
            Parameters: [new("@class", "int", "int", "", HasStringConvenience: false)],
            Documentation: null);

        BindingDocs.Function(output, function);

        var docs = output.ToString();
        StringAssert.Contains(docs, "<summary>Calls native <c>test_flag</c>.</summary>");
        StringAssert.Contains(docs, "<param name=\"class\">Native <c>class</c> parameter for <c>test_flag</c>.</param>");
        StringAssert.Contains(docs, "<returns>true when <c>test_flag</c> returns non-zero; otherwise, false.</returns>");
    }

    /// <summary>Native summaries keep an existing symbol anchor instead of duplicating it.</summary>
    [TestMethod]
    public void NativeSummary_AlreadyAnchoredByNativeName_DoesNotRepeatNativeName()
    {
        var docs = XmlDocComment.NativeSummary("XXH_VERSION_MAJOR", "<c>XXH_VERSION_MAJOR</c> - Version", "fallback.");

        Assert.AreEqual("<c>XXH_VERSION_MAJOR</c> - Version.", docs);
    }

    /// <summary>Native summaries code-tag a bare leading native name without adding a duplicate prefix.</summary>
    [TestMethod]
    public void NativeSummary_BareLeadingNativeName_CodeTagsWithoutRepeatingNativeName()
    {
        var docs = XmlDocComment.NativeSummary("XXH_VERSION_MINOR", "XXH_VERSION_MINOR - Version minor", "fallback.");

        Assert.AreEqual("<c>XXH_VERSION_MINOR</c> - Version minor.", docs);
    }
}
