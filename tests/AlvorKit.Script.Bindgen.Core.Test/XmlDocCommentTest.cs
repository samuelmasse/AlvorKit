using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

[TestClass]
public sealed class XmlDocCommentTest
{
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

    [TestMethod]
    public void Member_StripsTrailingCommentMarkup()
    {
        Assert.AreEqual("left mouse button", XmlDocComment.Member("/*!< [left mouse button](@ref buttons) */"));
    }
}
