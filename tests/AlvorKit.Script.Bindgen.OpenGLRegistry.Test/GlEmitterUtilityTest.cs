using System.Text;

namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for shared OpenGL registry source-emission helpers.</summary>
[TestClass]
public sealed class GlEmitterUtilityTest
{
    /// <summary>Command docs capitalize summaries, preserve terminal punctuation, and include parameter documentation.</summary>
    [TestMethod]
    public void CommandDocEmitter_WithDocumentation_FormatsSummaryAndParams()
    {
        var command = new GlCommand(
            "glFixture",
            "Fixture",
            "void",
            "void",
            [new("mode", "mode", "GlEnum", "uint", null, 0, null, false, false)],
            new("1.0", "2.0"),
            new("does work!", new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "Selects the mode." }, null, null),
            ReturnsCString: false);
        var output = new StringBuilder();

        GlCommandDocEmitter.Emit(output, command);

        var source = output.ToString();
        StringAssert.Contains(source, "<c>glFixture</c> (GL 1.0, GL ES 2.0) - Does work!");
        StringAssert.Contains(source, "<param name=\"mode\">Selects the mode.</param>");
    }

    /// <summary>Signature formatting handles void calls, unchanged returns, bool conversions, and enum casts.</summary>
    [TestMethod]
    public void SignatureFormatter_FormatsCallsAndConversions()
    {
        var boolCommand = Command(
            "glIsEnabled",
            "IsEnabled",
            "bool",
            "byte",
            [new("cap", "cap", "GlEnum", "uint", null, 0, null, false, false)]);
        var enumCommand = Command("glGetEnum", "GetEnum", "GlEnum", "uint", []);
        var voidCommand = Command(
            "glEnable",
            "Enable",
            "void",
            "void",
            [new("enabled", "enabled", "bool", "byte", null, 0, null, false, false)]);
        var sameReturnCommand = Command("glGetError", "GetError", "uint", "uint", []);

        Assert.AreEqual("GlEnum cap", GlSignatureFormatter.Signature(boolCommand));
        Assert.AreEqual("delegate* unmanaged<uint, byte>", GlSignatureFormatter.DelegateType(boolCommand));
        Assert.AreEqual("return glIsEnabled((uint)cap) != 0;", GlSignatureFormatter.BackendCall(boolCommand));
        Assert.AreEqual("return (GlEnum)glGetEnum();", GlSignatureFormatter.BackendCall(enumCommand));
        Assert.AreEqual("glEnable(enabled ? (byte)1 : (byte)0);", GlSignatureFormatter.BackendCall(voidCommand));
        Assert.AreEqual("return glGetError();", GlSignatureFormatter.BackendCall(sameReturnCommand));
    }

    /// <summary>Third-party notices distinguish no-doc output from Khronos attribution output.</summary>
    [TestMethod]
    public void ThirdPartyNoticeEmitter_EmitsNoDocAndDocumentedNotice()
    {
        var context = new GlCodeEmissionContext(OpenGlRegistryTestConfig.Create(), "registry-tag-abcdef", "documentation-tag-abcdef");
        var emitter = new GlThirdPartyNoticeEmitter(context);
        var noDocs = new GlBindingModel([], new("GLenum", "GlEnum", IsFlags: false, Members: []), null, [], [], [], [], []);
        var withDocs = new GlBindingModel(
            [],
            new("GLenum", "GlEnum", IsFlags: false, Members: []),
            null,
            [Command("glFixture", "Fixture", "void", "void", [], new("1.0", null), new("does work", [], null, null), false)],
            [],
            [],
            [],
            []);

        StringAssert.Contains(emitter.Emit(noDocs), "contains no third-party content");
        var notice = emitter.Emit(withDocs);

        StringAssert.Contains(notice, "AlvorKit.Bindgen.OpenGLFixture");
        StringAssert.Contains(notice, "1 of");
        StringAssert.Contains(notice, "documentation");
    }

    /// <summary>Creates a command for formatter tests.</summary>
    private static GlCommand Command(
        string nativeName,
        string managedName,
        string returnType,
        string returnInteropType,
        IReadOnlyList<GlParameter> parameters,
        GlAvailability? availability = null,
        XmlDocComment? documentation = null,
        bool returnsCString = false) =>
        new(nativeName, managedName, returnType, returnInteropType, parameters, availability ?? new("1.0", null), documentation, returnsCString);
}
