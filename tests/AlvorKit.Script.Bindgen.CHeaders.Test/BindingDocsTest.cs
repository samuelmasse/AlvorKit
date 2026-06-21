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
        StringAssert.Contains(docs, "/// <remarks>");
        StringAssert.Contains(docs, "/// Important.");
        Assert.IsFalse(docs.Contains("/// <c>test_value</c>: Important.", StringComparison.Ordinal));
        StringAssert.Contains(docs, "/// </remarks>");
    }

    /// <summary>Long function remarks become sectioned, wrapped XML paragraphs instead of one source line.</summary>
    [TestMethod]
    public void Function_LongRemarks_EmitsWrappedParagraphs()
    {
        var output = new StringBuilder();
        var function = new BindingFunction(
            NativeName: "test_open",
            ManagedName: "Open",
            ReturnType: "void",
            ReturnInteropType: "void",
            Parameters: [],
            Documentation: new(
                "opens a handle",
                [],
                null,
                "Starts with a long explanation that should be wrapped across several XML documentation lines " +
                "without losing the section breaks. Parameters handle The native handle. " +
                "Return Value Nothing. Thread Safety Unsafe. See Also test_close()"));

        BindingDocs.Function(output, function);

        var docs = output.ToString();
        StringAssert.Contains(docs, "/// Starts with a long explanation");
        StringAssert.Contains(docs, "/// Parameters: handle The native handle.");
        StringAssert.Contains(docs, "/// Return Value: Nothing.");
        StringAssert.Contains(docs, "/// Thread Safety: Unsafe.");
        StringAssert.Contains(docs, "/// See Also: test_close()");
        Assert.IsFalse(docs.Contains("<c>test_open</c>: Starts", StringComparison.Ordinal));
        Assert.IsTrue(docs.Split(Environment.NewLine).All(line => line.Length <= 170));
    }

    /// <summary>Long unbroken remark text wraps at the configured column without exceeding repository line length.</summary>
    [TestMethod]
    public void Function_LongUnbrokenRemarks_WrapsAtColumn()
    {
        var output = new StringBuilder();
        var longWord = new string('x', 150);
        var function = new BindingFunction(
            NativeName: "test_wrap",
            ManagedName: "Wrap",
            ReturnType: "void",
            ReturnInteropType: "void",
            Parameters: [],
            Documentation: new("wraps text", [], null, longWord));

        BindingDocs.Function(output, function);

        var docs = output.ToString();
        StringAssert.Contains(docs, $"/// {longWord[..130]}");
        StringAssert.Contains(docs, $"/// {longWord[130..]}");
        Assert.IsTrue(docs.Split(Environment.NewLine).All(line => line.Length <= 170));
    }

    /// <summary>Function remarks that begin with a section heading do not repeat the summary's native anchor.</summary>
    [TestMethod]
    public void Function_HeadingFirstRemarks_DoesNotRepeatNativeAnchor()
    {
        var output = new StringBuilder();
        var function = new BindingFunction(
            NativeName: "test_query",
            ManagedName: "Query",
            ReturnType: "int",
            ReturnInteropType: "int",
            Parameters: [],
            Documentation: new("queries state", [], null, "Parameters handle The native handle. Return Value A result."));

        BindingDocs.Function(output, function);

        var docs = output.ToString();
        StringAssert.Contains(docs, "/// Parameters: handle The native handle.");
        Assert.IsFalse(docs.Contains("/// <c>test_query</c>: Parameters", StringComparison.Ordinal));
        StringAssert.Contains(docs, "/// Return Value: A result.");
    }

    /// <summary>Standalone summary and parameter helpers wrap XML documentation blocks with the requested indentation.</summary>
    [TestMethod]
    public void SummaryAndParameter_LongText_EmitWrappedBlocks()
    {
        var summary = BindingDocs.Summary(
            "Native <c>test_value</c> documentation that should wrap cleanly when emitted as a standalone generated type summary.",
            "    ");
        var parameter = BindingDocs.Parameter(
            "value",
            "Native <c>value</c> parameter documentation that should wrap cleanly in delegate XML documentation.",
            "    ");

        StringAssert.Contains(summary, "    /// <summary>");
        StringAssert.Contains(summary, "    /// Native <c>test_value</c> documentation");
        StringAssert.Contains(summary, "    /// </summary>");
        StringAssert.Contains(parameter, "    /// <param name=\"value\">");
        StringAssert.Contains(parameter, "    /// Native <c>value</c> parameter documentation");
        StringAssert.Contains(parameter, "    /// </param>");
        Assert.IsTrue((summary + parameter).Split(Environment.NewLine).All(line => line.Length <= 170));
    }

    /// <summary>Standalone summary docs wrap spaced text and still emit an empty element for empty input.</summary>
    [TestMethod]
    public void Summary_LongSpacedAndEmptyText_EmitExpectedBlocks()
    {
        var longSpaced = string.Join(' ', new[]
        {
            "Native", "documentation", "with", "many", "short", "words", "that", "should", "wrap", "at", "a", "space",
            "instead", "of", "falling", "back", "to", "the", "hard", "column", "split", "used", "for", "unbroken", "text.",
        });

        var summary = BindingDocs.Summary(longSpaced, "    ");
        var emptySummary = BindingDocs.Summary("", "    ");

        StringAssert.Contains(summary, "    /// Native documentation with many short words");
        StringAssert.Contains(emptySummary, "    /// <summary>");
        StringAssert.Contains(emptySummary, "    /// </summary>");
        Assert.IsTrue((summary + emptySummary).Split(Environment.NewLine).All(line => line.Length <= 170));
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
