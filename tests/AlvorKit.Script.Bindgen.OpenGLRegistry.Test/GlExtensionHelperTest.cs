namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for small OpenGL extension helper utilities and guards.</summary>
[TestClass]
public sealed class GlExtensionHelperTest
{
    /// <summary>Len parsing covers absent, literal, parameter, multiplier, COMPSIZE, and unknown metadata.</summary>
    [TestMethod]
    public void LenParser_ParsesKnownAndUnknownShapes()
    {
        var command = Command([
            Parameter("count", "count", "int", "int"),
            Parameter("pointerCount", "pointerCount", "nint", "nint", pointerDepth: 1),
            Parameter("literal", "literal", "nint", "nint", "4", 1),
            Parameter("byCount", "byCount", "nint", "nint", "count", 1),
            Parameter("byCountMultiplier", "byCountMultiplier", "nint", "nint", "count*4", 1),
            Parameter("compSize", "compSize", "nint", "nint", "COMPSIZE(width,height)", 1),
            Parameter("badSyntax", "badSyntax", "nint", "nint", "count + 1", 1),
            Parameter("badReference", "badReference", "nint", "nint", "missing", 1),
            Parameter("pointerReference", "pointerReference", "nint", "nint", "pointerCount", 1)
        ]);

        Assert.AreEqual(GlExtensionLenKind.None, GlExtensionLenParser.Parse(command, command.Parameters[0]).Kind);
        Assert.AreEqual(new(GlExtensionLenKind.Literal, -1, 4, []), GlExtensionLenParser.Parse(command, command.Parameters[2]));
        Assert.AreEqual(new(GlExtensionLenKind.ParamRef, 0, 1, []), GlExtensionLenParser.Parse(command, command.Parameters[3]));
        Assert.AreEqual(new(GlExtensionLenKind.ParamRef, 0, 4, []), GlExtensionLenParser.Parse(command, command.Parameters[4]));
        CollectionAssert.AreEqual(new[] { "width", "height" }, GlExtensionLenParser.Parse(command, command.Parameters[5]).CompSizeArgs);
        Assert.AreEqual(GlExtensionLenKind.Unknown, GlExtensionLenParser.Parse(command, command.Parameters[6]).Kind);
        Assert.AreEqual(GlExtensionLenKind.Unknown, GlExtensionLenParser.Parse(command, command.Parameters[7]).Kind);
        Assert.AreEqual(GlExtensionLenKind.Unknown, GlExtensionLenParser.Parse(command, command.Parameters[8]).Kind);
    }

    /// <summary>Extension naming helpers cover escaping, generic type names, count casts, and singular helper names.</summary>
    [TestMethod]
    public void ExtensionNames_FormatCommonGeneratedFragments()
    {
        var config = OpenGlRegistryTestConfig.Create();
        var command = Command([
            Parameter("count", "count", "uint", "uint"),
            Parameter("data", "data", "nint", "nint", pointerDepth: 1),
            Parameter("indices", "indices", "nint", "nint", pointerDepth: 1)
        ]);

        Assert.AreEqual("event", GlExtensionNames.Local(Parameter("event", "@event", "int", "int")));
        Assert.AreEqual("Gl.Fixture(uint, nint, nint)", GlExtensionNames.CoreCref(config, command));
        Assert.AreEqual("<paramref name=\"event\"/>, <paramref name=\"mode\"/>", GlExtensionNames.ParamRefs(["@event", "mode"]));
        Assert.AreEqual("<c>event</c>, <c>mode</c>", GlExtensionNames.CodeNames(["@event", "mode"]));
        Assert.AreEqual("ReadOnlySpan<int>", GlExtensionNames.SpanType(Parameter("values", "values", "nint", "nint", pointeeIsConst: true), "int"));
        Assert.AreEqual("Span<int>", GlExtensionNames.SpanType(Parameter("values", "values", "nint", "nint"), "int"));
        Assert.AreEqual("TData", GlExtensionNames.TypeParameter(command, 1));
        Assert.AreEqual("(uint)(items.Length)", GlExtensionNames.CountExpression(command.Parameters[0], "items.Length"));
        Assert.AreEqual("Entry", GlExtensionNames.Depluralize("Entries"));
        Assert.AreEqual("Buffer", GlExtensionNames.Depluralize("Buffers"));
        Assert.AreEqual("Class", GlExtensionNames.Depluralize("Class"));
    }

    /// <summary>Managed name guard allows unique projections and reports colliding native names.</summary>
    [TestMethod]
    public void ManagedNameGuard_ReportsCollisions()
    {
        GlManagedNameGuard.AssertUnique([("Buffer", "GL_BUFFER"), ("Texture", "GL_TEXTURE")], "enum");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => GlManagedNameGuard.AssertUnique(
            [("Buffer", "GL_BUFFER"), ("Buffer", "GL_BUFFER_EXT")],
            "enum"));

        StringAssert.Contains(exception.Message, "Colliding managed enum names");
        StringAssert.Contains(exception.Message, "Buffer (GL_BUFFER, GL_BUFFER_EXT)");
    }

    /// <summary>Binding model diagnostic collections stay available to callers.</summary>
    [TestMethod]
    public void BindingModel_ExposesDiagnostics()
    {
        var model = new GlBindingModel(
            [],
            new("GLenum", "GlEnum", IsFlags: false, Members: []),
            null,
            [],
            ["glFixture(mode: no group)"],
            ["glSkipped: configured"],
            [],
            []);

        CollectionAssert.Contains(model.UngroupedEnumUses.ToArray(), "glFixture(mode: no group)");
        CollectionAssert.Contains(model.SkippedCommands.ToArray(), "glSkipped: configured");
    }

    /// <summary>Creates a command with the standard void return and OpenGL 1.0 availability.</summary>
    private static GlCommand Command(IReadOnlyList<GlParameter> parameters) =>
        new("glFixture", "Fixture", "void", "void", parameters, new("1.0", null), Documentation: null, ReturnsCString: false);

    /// <summary>Creates an OpenGL command parameter.</summary>
    private static GlParameter Parameter(
        string nativeName,
        string managedName,
        string managedType,
        string interopType,
        string? len = null,
        int pointerDepth = 0,
        string? pointeeType = null,
        bool pointeeIsConst = false) =>
        new(nativeName, managedName, managedType, interopType, len, pointerDepth, pointeeType, pointeeIsConst, PointeeIsChar: false);
}
