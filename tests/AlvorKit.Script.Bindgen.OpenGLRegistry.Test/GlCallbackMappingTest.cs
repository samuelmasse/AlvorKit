namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for OpenGL callback typedef parsing and managed delegate mapping.</summary>
[TestClass]
public sealed class GlCallbackMappingTest
{
    /// <summary>Callback typedef parsing handles empty config, void parameter lists, and invalid typedef text.</summary>
    [TestMethod]
    public void TypedefParser_HandlesConfiguredAndInvalidCallbacks()
    {
        var empty = new GlCallbackTypedefParser(OpenGlRegistryTestConfig.Create()).Parse(new XElement("registry"));
        Assert.AreEqual(0, empty.Signatures.Count);

        var config = OpenGlRegistryTestConfig.Create();
        config.Callbacks["GLDEBUGPROC"] = new() { ManagedName = "GlDebugProc" };
        var registry = XElement.Parse("""
            <registry>
              <types>
                <type>typedef void (APIENTRY *<name>GLDEBUGPROC</name>)(void);</type>
                <type>typedef <ptype>void</ptype> <name>NotACallback</name>;</type>
              </types>
            </registry>
            """);

        var parsed = new GlCallbackTypedefParser(config).Parse(registry);
        Assert.AreEqual("void", parsed.Signatures["GLDEBUGPROC"].ReturnType);
        Assert.AreEqual(0, parsed.Signatures["GLDEBUGPROC"].Parameters.Count);

        config.Callbacks["Missing"] = new() { ManagedName = "MissingCallback" };
        var missing = Assert.ThrowsExactly<InvalidOperationException>(() => new GlCallbackTypedefParser(config).Parse(registry));
        StringAssert.Contains(missing.Message, "Missing");

        config.Callbacks.Remove("Missing");
        config.Callbacks["NotACallback"] = new() { ManagedName = "BrokenCallback" };
        var invalid = Assert.ThrowsExactly<InvalidOperationException>(() => new GlCallbackTypedefParser(config).Parse(registry));
        StringAssert.Contains(invalid.Message, "NotACallback is not a function-pointer typedef");
    }

    /// <summary>Callback delegate mapping covers pointer returns, grouped enums, catch-all enums, booleans, and char pointers.</summary>
    [TestMethod]
    public void DelegateBuilder_MapsCallbackShapes()
    {
        var config = OpenGlRegistryTestConfig.Create();
        config.Callbacks["GLDEBUGPROC"] = new()
        {
            ManagedName = "GlDebugProc",
            ParamGroups = { ["type"] = "DebugType" }
        };
        var signatures = new Dictionary<string, GlCallbackSignature>(StringComparer.Ordinal)
        {
            ["GLDEBUGPROC"] = new(
                "const GLchar *",
                [
                    new("GLenum", "type"),
                    new("GLenum", "severity"),
                    new("GLboolean", "enabled"),
                    new("const GLchar *", "message")
                ])
        };
        var groups = new Dictionary<string, string>(StringComparer.Ordinal) { ["DebugType"] = "GlDebugType" };

        var delegates = new GlCallbackDelegateBuilder(config, "GlEnum").Build(signatures, new HashSet<string>(["GLDEBUGPROC"]), groups);
        var callback = delegates.Single();

        Assert.AreEqual("nint", callback.ReturnType);
        Assert.AreEqual("GlDebugType", callback.Parameters[0].ManagedType);
        Assert.AreEqual("GlEnum", callback.Parameters[1].ManagedType);
        Assert.AreEqual("byte", callback.Parameters[2].ManagedType);
        Assert.AreEqual("nint", callback.Parameters[3].ManagedType);
        Assert.IsTrue(callback.Parameters[3].PointeeIsConst);
        Assert.IsTrue(callback.Parameters[3].PointeeIsChar);
    }

    /// <summary>Callback delegate mapping throws for unmapped return and parameter types.</summary>
    [TestMethod]
    public void DelegateBuilder_UnmappedTypes_Throw()
    {
        var config = OpenGlRegistryTestConfig.Create();
        config.Callbacks["BadReturn"] = new() { ManagedName = "BadReturn" };
        config.Callbacks["BadParam"] = new() { ManagedName = "BadParam" };
        var builder = new GlCallbackDelegateBuilder(config, "GlEnum");

        var badReturn = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build(
            new Dictionary<string, GlCallbackSignature>(StringComparer.Ordinal)
            {
                ["BadReturn"] = new("BadType", [])
            },
            new HashSet<string>(["BadReturn"]),
            new Dictionary<string, string>(StringComparer.Ordinal)));
        var badParam = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build(
            new Dictionary<string, GlCallbackSignature>(StringComparer.Ordinal)
            {
                ["BadParam"] = new("void", [new("BadType", "value")])
            },
            new HashSet<string>(["BadParam"]),
            new Dictionary<string, string>(StringComparer.Ordinal)));

        StringAssert.Contains(badReturn.Message, "BadReturn");
        StringAssert.Contains(badParam.Message, "BadType");
    }
}
