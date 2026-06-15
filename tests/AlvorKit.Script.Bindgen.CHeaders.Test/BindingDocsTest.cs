using System.Text;

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
        StringAssert.Contains(docs, "<param name=\"input\">Input value.</param>");
        StringAssert.Contains(docs, "<returns>The value.</returns>");
        StringAssert.Contains(docs, "<remarks>Important.</remarks>");
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
        StringAssert.Contains(docs, "<param name=\"class\">Native <c>class</c> parameter.</param>");
        StringAssert.Contains(docs, "<returns>true when the native function returns non-zero; otherwise, false.</returns>");
    }
}
