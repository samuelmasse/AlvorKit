using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for mapping OpenGL registry declarations into generated type shapes.</summary>
[TestClass]
public sealed class GlDeclarationMapperTest
{
    /// <summary>Pointer declarations preserve enum groups, handle classes, constness, and character pointee metadata.</summary>
    [TestMethod]
    public void Map_PointerDeclarations_MapTypedPointeeMetadata()
    {
        var mapper = new GlDeclarationMapper("GlEnum");
        var groups = new Dictionary<string, string>(StringComparer.Ordinal) { ["TextureTarget"] = "GlTextureTarget" };
        var handles = new SortedSet<string>(StringComparer.Ordinal);

        var enumPointer = mapper.Map(
            Declaration("""<param group="TextureTarget"><ptype>GLenum</ptype> *<name>targets</name></param>"""),
            "glFixture",
            groups,
            new Dictionary<string, string>(StringComparer.Ordinal),
            handles,
            [],
            []);
        var handlePointer = mapper.Map(
            Declaration("""<param class="buffer"><ptype>GLuint</ptype> *<name>buffers</name></param>"""),
            "glFixture",
            groups,
            new Dictionary<string, string>(StringComparer.Ordinal),
            handles,
            [],
            []);
        var charPointer = mapper.Map(
            Declaration("""<param>const <ptype>GLchar</ptype> *<name>label</name></param>"""),
            "glFixture",
            groups,
            new Dictionary<string, string>(StringComparer.Ordinal),
            handles,
            [],
            []);

        Assert.AreEqual("GlTextureTarget", enumPointer.PointeeType);
        Assert.AreEqual("GlBufferHandle", handlePointer.PointeeType);
        CollectionAssert.Contains(handles.ToArray(), "GlBufferHandle");
        Assert.IsTrue(charPointer.PointeeIsConst);
        Assert.IsTrue(charPointer.PointeeIsChar);
        Assert.AreEqual("byte", charPointer.PointeeType);
    }

    /// <summary>Scalar declarations handle configured callbacks, grouped ints, object handles, and ungrouped enums.</summary>
    [TestMethod]
    public void Map_ScalarDeclarations_MapSpecialShapes()
    {
        var mapper = new GlDeclarationMapper("GlEnum");
        var groups = new Dictionary<string, string>(StringComparer.Ordinal) { ["TextureTarget"] = "GlTextureTarget" };
        var callbacks = new Dictionary<string, string>(StringComparer.Ordinal) { ["GLDEBUGPROC"] = "GlDebugProc" };
        var handles = new SortedSet<string>(StringComparer.Ordinal);
        var ungrouped = new List<string>();
        var usedCallbacks = new HashSet<string>(StringComparer.Ordinal);

        var callback = mapper.Map(
            Declaration("<param><ptype>GLDEBUGPROC</ptype> <name>callback</name></param>"),
            "glFixture",
            groups,
            callbacks,
            handles,
            ungrouped,
            usedCallbacks);
        var groupedInt = mapper.Map(
            Declaration("""<param group="TextureTarget"><ptype>GLint</ptype> <name>target</name></param>"""),
            "glFixture",
            groups,
            callbacks,
            handles,
            ungrouped,
            usedCallbacks);
        var objectHandle = mapper.Map(
            Declaration("<param><ptype>GLuint</ptype> <name>name</name></param>"),
            "glFixture",
            groups,
            callbacks,
            handles,
            ungrouped,
            usedCallbacks,
            objectCommand: true);
        var ungroupedEnum = mapper.Map(
            Declaration("<param><ptype>GLenum</ptype> <name>mode</name></param>"),
            "glFixture",
            groups,
            callbacks,
            handles,
            ungrouped,
            usedCallbacks);

        Assert.AreEqual("GlDebugProc", callback.CallbackType);
        CollectionAssert.Contains(usedCallbacks.ToArray(), "GLDEBUGPROC");
        Assert.AreEqual("GlTextureTarget", groupedInt.Type.Managed);
        Assert.AreEqual("GlHandle", objectHandle.Type.Managed);
        Assert.AreEqual("GlEnum", ungroupedEnum.Type.Managed);
        CollectionAssert.Contains(ungrouped, "glFixture(mode: no group)");
    }

    /// <summary>Unmapped C types fail with the command name in the error.</summary>
    [TestMethod]
    public void Map_UnmappedType_Throws()
    {
        var mapper = new GlDeclarationMapper("GlEnum");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            mapper.Map(
                Declaration("<param><ptype>GLunknown</ptype> <name>value</name></param>"),
                "glFixture",
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, string>(StringComparer.Ordinal),
                new SortedSet<string>(StringComparer.Ordinal),
                [],
                []));

        StringAssert.Contains(exception.Message, "glFixture");
        StringAssert.Contains(exception.Message, "GLunknown");
    }

    /// <summary>Parses one registry declaration element from a compact XML fragment.</summary>
    private static XElement Declaration(string xml) => XElement.Parse(xml);
}
