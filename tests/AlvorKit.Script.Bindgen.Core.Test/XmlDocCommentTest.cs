namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers native comment normalization for generated XML docs.</summary>
[TestClass]
public sealed class XmlDocCommentTest
{
    /// <summary>Blank native comments do not produce documentation objects.</summary>
    [TestMethod]
    public void Parse_ReturnsNullForBlankComments()
    {
        Assert.IsNull(XmlDocComment.Parse(null));
        Assert.IsNull(XmlDocComment.Parse("   "));
    }

    /// <summary>Doxygen brief, parameter, and return directives become XML-doc-ready text.</summary>
    [TestMethod]
    public void Parse_NormalizesDoxygenSummaryParametersAndReturns()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Opens @ref GLFWwindow handles.
             * @param[in] window The @p window to open.
             * @param[out] width Receives [width](@ref size).
             * @return GLFW_TRUE on success.
             * @thread_safety Must be called from the main thread.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Opens GLFWwindow handles.", doc.Summary);
        Assert.AreEqual("The window to open.", doc.Parameters["window"]);
        Assert.AreEqual("Receives width.", doc.Parameters["width"]);
        Assert.AreEqual("GLFW_TRUE on success.", doc.Returns);
        Assert.IsNull(doc.Remarks);
    }

    /// <summary>Plain paragraphs before and after a blank line map to summary and remarks.</summary>
    [TestMethod]
    public void Parse_SplitsSummaryAndRemarksAtBlankLine()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * Opens the handle.
             *
             * More details with <xml> & text.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Opens the handle.", doc.Summary);
        Assert.AreEqual("More details with &lt;xml&gt; &amp; text.", doc.Remarks);
    }

    /// <summary>Multiline parameters are joined and escaped for XML documentation.</summary>
    [TestMethod]
    public void Parse_JoinsAndEscapesMultilineParameters()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Sets a value.
             * @param value First <value>
             *   and more & data.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("First &lt;value&gt; and more &amp; data.", doc.Parameters["value"]);
    }

    /// <summary>Group and metadata-only tags are ignored instead of leaking into IntelliSense.</summary>
    [TestMethod]
    public void Parse_IgnoresGroupsAndNoiseSections()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @defgroup fixtures Fixtures
             * @{
             * @brief Runs the fixture.
             * @sa fixture_init
             * This line should be discarded.
             * @return Zero on success.
             * @}
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Runs the fixture.", doc.Summary);
        Assert.AreEqual("Zero on success.", doc.Returns);
        Assert.IsNull(doc.Remarks);
    }

    /// <summary>FreeType section headers map to the same XML-doc fields as Doxygen directives.</summary>
    [TestMethod]
    public void Parse_HandlesFreeTypeSectionedComments()
    {
        var doc = XmlDocComment.Parse("""
            /**************************************************************************
             *
             * @description:
             *   Loads @FT_Face from @library.
             *
             * @input:
             *   face ::
             *     A face handle.
             *
             * @return:
             *   Zero on success.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Loads FT_Face from library.", doc.Summary);
        Assert.AreEqual("A face handle.", doc.Parameters["face"]);
        Assert.AreEqual("Zero on success.", doc.Returns);
    }

    /// <summary>FreeType notes become remarks and inout parameters are collected like inputs.</summary>
    [TestMethod]
    public void Parse_HandlesFreeTypeNotesAndInOutParameters()
    {
        var doc = XmlDocComment.Parse("""
            /**************************************************************************
             *
             * @description:
             *   Updates @FT_Face.
             *
             * @inout:
             *   face ::
             *     A face handle.
             *
             * @note:
             *   Uses ~library storage.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Updates FT_Face.", doc.Summary);
        Assert.AreEqual("A face handle.", doc.Parameters["face"]);
        Assert.AreEqual("Uses library storage.", doc.Remarks);
    }

}
