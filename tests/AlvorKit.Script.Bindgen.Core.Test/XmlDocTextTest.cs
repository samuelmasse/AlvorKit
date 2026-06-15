namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers low-level native comment text cleanup.</summary>
[TestClass]
public sealed class XmlDocTextTest
{
    /// <summary>Comment delimiters and leading decoration are removed from raw native lines.</summary>
    [TestMethod]
    public void CleanLine_StripsNativeCommentDecoration()
    {
        Assert.AreEqual("value", XmlDocComment.CleanLine("/*!< value */"));
        Assert.AreEqual("value", XmlDocComment.CleanLine(" * -- value"));
        Assert.AreEqual("value", XmlDocComment.CleanLine(" * <value"));
        Assert.AreEqual("Canonical representation", XmlDocComment.CleanLine("/** Canonical representation ******/"));
    }

    /// <summary>Member comments are reduced to single-line XML-doc-safe prose.</summary>
    [TestMethod]
    public void Member_StripsTrailingCommentMarkup()
    {
        Assert.AreEqual("left mouse button", XmlDocComment.Member("/*!< [left mouse button](@ref buttons) */"));
        Assert.IsNull(XmlDocComment.Member("/** @note */"));
        Assert.IsNull(XmlDocComment.Member("   "));
    }

    /// <summary>The public XML-doc escaping helper protects generated documentation text.</summary>
    [TestMethod]
    public void Escape_EncodesXmlSpecialCharacters()
    {
        Assert.AreEqual("A &amp; B &lt; C &gt; D", XmlDocComment.Escape("A & B < C > D"));
    }

    /// <summary>Adjacent prose fragments are joined with a single sentence space.</summary>
    [TestMethod]
    public void Parse_JoinsAdjacentSummaryFragments()
    {
        var doc = XmlDocComment.Parse("""
            /**
             * First sentence.
             * Second sentence.
             */
            """);

        Assert.IsNotNull(doc);
        Assert.AreEqual("First sentence. Second sentence.", doc.Summary);
    }

    /// <summary>Comments that contain only stripped markup produce no summary text.</summary>
    [TestMethod]
    public void Parse_DropsSummaryWhenOnlyMarkupRemains()
    {
        var doc = XmlDocComment.Parse("/** @brief @note */");

        Assert.IsNotNull(doc);
        Assert.IsNull(doc.Summary);
    }
}
