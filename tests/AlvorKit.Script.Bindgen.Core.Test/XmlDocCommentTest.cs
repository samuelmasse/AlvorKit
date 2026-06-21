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
        Assert.AreEqual("Opens <c>GLFWwindow</c> handles.", doc.Summary);
        Assert.AreEqual("The window to open.", doc.Parameters["window"]);
        Assert.AreEqual("Receives width.", doc.Parameters["width"]);
        Assert.AreEqual("<c>GLFW_TRUE</c> on success.", doc.Returns);
        Assert.IsNull(doc.Remarks);
    }

    /// <summary>Native C-family symbols in GLFW prose become exact XML code anchors.</summary>
    [TestMethod]
    public void Parse_AnchorsGlfwAndVulkanNativeReferences()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Calls glfwSetInputMode with GLFW_CURSOR.
             *
             * Returns GLFW_TRUE or VK_ERROR_EXTENSION_NOT_PRESENT. Call vkDestroySurfaceKHR with NULL and VkInstanceCreateInfo.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Calls <c>glfwSetInputMode</c> with <c>GLFW_CURSOR</c>.", doc.Summary);
        Assert.AreEqual(
            "Returns <c>GLFW_TRUE</c> or <c>VK_ERROR_EXTENSION_NOT_PRESENT</c>. " +
            "Call <c>vkDestroySurfaceKHR</c> with <c>NULL</c> and <c>VkInstanceCreateInfo</c>.",
            doc.Remarks);
    }

    /// <summary>Doxygen page references become readable prose rather than raw generated site anchors.</summary>
    [TestMethod]
    public void Parse_HumanizesNonSymbolDoxygenReferences()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief See @ref context_sharing and @ref window_windowed_full_screen.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("See context sharing and windowed full screen.", doc.Summary);
    }

    /// <summary>Repeated native function names in one prose section are all preserved as exact code anchors.</summary>
    [TestMethod]
    public void Parse_AnchorsRepeatedGlfwFunctionNames()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Reads buttons.
             *
             * Earlier versions did not have glfwGetJoystickHats. The hats are in the same order as returned by
             * __glfwGetJoystickHats__ and are in the order _up_, _right_, _down_ and _left_.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual(
            "Earlier versions did not have <c>glfwGetJoystickHats</c>. The hats are in the same order as returned by " +
            "<c>glfwGetJoystickHats</c> and are in the order up, right, down and left.",
            doc.Remarks);
    }

    /// <summary>Markdown links whose visible labels are native functions keep those labels as code anchors.</summary>
    [TestMethod]
    public void Parse_AnchorsNativeNamesFromMarkdownLinkLabels()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Reads buttons.
             *
             * The order matches [glfwGetJoystickHats](@ref input_joystick_hat).
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("The order matches <c>glfwGetJoystickHats</c>.", doc.Remarks);
    }

    /// <summary>Markdown reference definitions and table rows are omitted from flattened XML docs.</summary>
    [TestMethod]
    public void Parse_DropsMarkdownReferenceDefinitionsAndTables()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Reads monitor data.
             *
             * See the table below for details.
             * Name | Value
             * ---- | -----
             * GLFW_HAT_UP | 1
             * 1) Theme-dependent.
             * [EDID]: https://example.test/edid
             * Call glfwGetMonitors afterwards.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Reads monitor data.", doc.Summary);
        Assert.AreEqual("Call <c>glfwGetMonitors</c> afterwards.", doc.Remarks);
    }

    /// <summary>Table lead-in text is also removed from parameter documentation.</summary>
    [TestMethod]
    public void Parse_DropsTableLeadInsFromParameters()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Reads hats.
             * @param hats Each element in the array is one of the following values:
             * Name | Value
             * ---- | -----
             * GLFW_HAT_UP | 1
             * @param count Receives the hat count.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("", doc.Parameters["hats"]);
        Assert.AreEqual("Receives the hat count.", doc.Parameters["count"]);
    }

    /// <summary>Native summaries use the fallback text when no native documentation exists.</summary>
    [TestMethod]
    public void NativeSummary_UsesFallbackWhenOriginalIsMissing()
    {
        Assert.AreEqual("Fallback summary.", XmlDocComment.NativeSummary("glfwInit", null, "Fallback summary."));
    }

    /// <summary>Native summaries preserve original prose that is already anchored to the native symbol.</summary>
    [TestMethod]
    public void NativeSummary_PreservesAlreadyAnchoredOriginal()
    {
        Assert.AreEqual(
            "<c>glfwInit</c> initializes GLFW.",
            XmlDocComment.NativeSummary("glfwInit", "<c>glfwInit</c> initializes GLFW", "Fallback."));
    }

    /// <summary>Native summaries anchor a bare native prefix when it ends at a symbol boundary.</summary>
    [TestMethod]
    public void NativeSummary_AnchorsBareNativePrefix()
    {
        Assert.AreEqual(
            "<c>glfwInit</c> initializes GLFW.",
            XmlDocComment.NativeSummary("glfwInit", "glfwInit initializes GLFW", "Fallback."));
    }

    /// <summary>Native summaries prepend the native symbol when the original text is not already anchored.</summary>
    [TestMethod]
    public void NativeSummary_PrependsNativeSymbolForUnanchoredOriginal()
    {
        Assert.AreEqual(
            "<c>glfwInit</c> - Initializes GLFW.",
            XmlDocComment.NativeSummary("glfwInit", "initializes GLFW", "Fallback."));
        Assert.AreEqual(
            "<c>glfwInit</c> - GlfwInitialize compatibility.",
            XmlDocComment.NativeSummary("glfwInit", "glfwInitialize compatibility", "Fallback."));
    }

    /// <summary>Native summaries recognize punctuation that can follow an anchored native symbol.</summary>
    [TestMethod]
    public void NativeSummary_RecognizesNativeNameBoundaryPunctuation()
    {
        Assert.AreEqual("<c>glfwInit</c>.", XmlDocComment.NativeSummary("glfwInit", "glfwInit.", "Fallback."));
        Assert.AreEqual("<c>glfwInit</c>: ready.", XmlDocComment.NativeSummary("glfwInit", "glfwInit: ready", "Fallback."));
        Assert.AreEqual("<c>glfwInit</c>-ready.", XmlDocComment.NativeSummary("glfwInit", "glfwInit-ready", "Fallback."));
        Assert.AreEqual("<c>glfwInit</c>, ready.", XmlDocComment.NativeSummary("glfwInit", "glfwInit, ready", "Fallback."));
        Assert.AreEqual("<c>glfwInit</c>; ready.", XmlDocComment.NativeSummary("glfwInit", "glfwInit; ready", "Fallback."));
        Assert.AreEqual("<c>glfwInit</c>) ready.", XmlDocComment.NativeSummary("glfwInit", "glfwInit) ready", "Fallback."));
    }

    /// <summary>Bare FreeType-family Doxygen symbol references become exact native code anchors.</summary>
    [TestMethod]
    public void Parse_AnchorsBareFreeTypeNativeReferences()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Wraps @FT_Outline_Embolden and @FT_Bitmap_Embolden.
             * References @ref FT_Face too.
             * Bold **OR** stray not** markers are normalized.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual(
            "Wraps <c>FT_Outline_Embolden</c> and <c>FT_Bitmap_Embolden</c>. References <c>FT_Face</c> too. " +
            "Bold OR stray not markers are normalized.",
            doc.Summary);
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

    /// <summary>Code blocks and their example lead-ins are discarded instead of leaving dangling prose.</summary>
    [TestMethod]
    public void Parse_DropsCodeBlockExampleLeadIn()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Generates a secret.
             * @param seed The generated secret can be used with @p seed. Example C++ string hash class:
             * @code{.cpp}
             * struct Hash {};
             * @endcode
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Generates a secret.", doc.Summary);
        Assert.AreEqual("The generated secret can be used with seed.", doc.Parameters["seed"]);
    }

    /// <summary>Code block lead-ins are also dropped from regular prose sections.</summary>
    [TestMethod]
    public void Parse_DropsSummaryCodeBlockExampleLeadIn()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * Uses the helper. Example C setup:
             * @code
             * helper();
             * @endcode
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Uses the helper.", doc.Summary);
    }

    /// <summary>Malformed parameter tags do not create empty parameter documentation.</summary>
    [TestMethod]
    public void Parse_IgnoresMalformedParameterTags()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * @brief Runs the helper.
             * @param
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("Runs the helper.", doc.Summary);
        Assert.AreEqual(0, doc.Parameters.Count);
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
        Assert.AreEqual("Loads <c>FT_Face</c> from library.", doc.Summary);
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
        Assert.AreEqual("Updates <c>FT_Face</c>.", doc.Summary);
        Assert.AreEqual("A face handle.", doc.Parameters["face"]);
        Assert.AreEqual("Uses library storage.", doc.Remarks);
    }

}
