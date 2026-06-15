namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Core tests for OpenGL registry parsing and type mapping.</summary>
[TestClass]
public sealed class GlRegistryParserTest
{
    /// <summary>Bitmask groups become typed flags enums and typed command parameters.</summary>
    [TestMethod]
    public void Parse_MapsGroupedBitfieldParametersToTypedFlagEnums()
    {
        using var workspace = TempWorkspace.Create();
        var registry = workspace.WriteFile("gl.xml", """
            <registry>
              <enums type="bitmask">
                <enum name="GL_COLOR_BUFFER_BIT" value="0x00004000" group="ClearBufferMask" />
              </enums>
              <commands>
                <command>
                  <proto>void <name>glClear</name></proto>
                  <param group="ClearBufferMask"><ptype>GLbitfield</ptype> <name>mask</name></param>
                </command>
              </commands>
              <feature api="gl" name="GL_VERSION_1_0" number="1.0">
                <require>
                  <enum name="GL_COLOR_BUFFER_BIT" />
                  <command name="glClear" />
                </require>
              </feature>
            </registry>
            """);

        var model = new GlRegistryParser(OpenGlRegistryTestConfig.Create()).Parse(registry, new Dictionary<string, XmlDocComment>());

        var group = model.Groups.Single();
        Assert.AreEqual("GlClearBufferMask", group.ManagedName);
        Assert.IsTrue(group.IsFlags);
        Assert.AreEqual("GlClearBufferMask", model.Commands.Single().Parameters.Single().ManagedType);
        CollectionAssert.Contains(model.AllTokens.Members.Single().Groups.ToList(), "GlClearBufferMask");
    }

    /// <summary>Core profile removal blocks remove previously selected symbols.</summary>
    [TestMethod]
    public void Parse_AppliesCoreProfileRemovals()
    {
        using var workspace = TempWorkspace.Create();
        var registry = workspace.WriteFile("gl.xml", """
            <registry>
              <enums>
                <enum name="GL_DRAW_BUFFER" value="0x0C01" />
              </enums>
              <commands>
                <command><proto>void <name>glDrawBuffer</name></proto></command>
              </commands>
              <feature api="gl" name="GL_VERSION_1_0" number="1.0">
                <require>
                  <enum name="GL_DRAW_BUFFER" />
                  <command name="glDrawBuffer" />
                </require>
              </feature>
              <feature api="gl" name="GL_VERSION_1_1" number="1.1">
                <remove profile="core">
                  <enum name="GL_DRAW_BUFFER" />
                  <command name="glDrawBuffer" />
                </remove>
              </feature>
            </registry>
            """);

        var model = new GlRegistryParser(OpenGlRegistryTestConfig.Create(glVersion: "1.1")).Parse(registry, new Dictionary<string, XmlDocComment>());

        Assert.AreEqual(0, model.Commands.Count);
        Assert.AreEqual(0, model.AllTokens.Members.Count);
    }

    /// <summary>Configured callback typedefs are parsed and projected as managed delegates.</summary>
    [TestMethod]
    public void Parse_ConfiguredCallbackTypedefDoesNotNeedHardCodedValueType()
    {
        using var workspace = TempWorkspace.Create();
        var registry = workspace.WriteFile("gl.xml", """
            <registry>
              <types>
                <type>typedef void (APIENTRY *<name>GLFOOPROC</name>)(GLenum source, const GLchar *message);</type>
              </types>
              <enums>
                <enum name="GL_DEBUG_SOURCE_API" value="0x8246" group="DebugSource" />
              </enums>
              <commands>
                <command>
                  <proto>void <name>glFooCallback</name></proto>
                  <param><ptype>GLFOOPROC</ptype> <name>callback</name></param>
                </command>
              </commands>
              <feature api="gl" name="GL_VERSION_1_0" number="1.0">
                <require>
                  <enum name="GL_DEBUG_SOURCE_API" />
                  <command name="glFooCallback" />
                </require>
              </feature>
            </registry>
            """);
        var config = OpenGlRegistryTestConfig.Create();
        config.Callbacks = new()
        {
            ["GLFOOPROC"] = new CallbackConfig
            {
                ManagedName = "GlFooProc",
                ParamGroups = new() { ["source"] = "DebugSource" }
            }
        };

        var model = new GlRegistryParser(config).Parse(registry, new Dictionary<string, XmlDocComment>());

        Assert.AreEqual("GlFooProc", model.Commands.Single().Parameters.Single().CallbackType);
        var callback = model.Delegates.Single();
        Assert.AreEqual("GlFooProc", callback.ManagedName);
        Assert.AreEqual("GlDebugSource", callback.Parameters[0].ManagedType);
        Assert.AreEqual("nint", callback.Parameters[1].ManagedType);
        Assert.IsTrue(callback.Parameters[1].PointeeIsChar);
    }

}
