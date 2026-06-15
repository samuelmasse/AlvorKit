namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Additional parser coverage for OpenGL registry selection and type mapping.</summary>
[TestClass]
public sealed class GlRegistryParserAdvancedTest
{
    /// <summary>Configured extensions are included and companion GL ES availability is recorded.</summary>
    [TestMethod]
    public void Parse_IncludesConfiguredExtensionsAndEsAvailability()
    {
        using var workspace = TempWorkspace.Create();
        var registry = workspace.WriteFile("gl.xml", """
            <registry>
              <enums>
                <enum name="GL_FOO_VALUE" value="0x1" group="FooGroup" />
              </enums>
              <commands>
                <command>
                  <proto>void <name>glFoo</name></proto>
                  <param group="FooGroup"><ptype>GLenum</ptype> <name>foo</name></param>
                </command>
              </commands>
              <feature api="gl" name="GL_VERSION_1_0" number="1.0"><require /></feature>
              <feature api="gles2" name="GL_ES_VERSION_3_0" number="3.0">
                <require><enum name="GL_FOO_VALUE" /><command name="glFoo" /></require>
              </feature>
              <extensions>
                <extension name="GL_TEST_extension">
                  <require><enum name="GL_FOO_VALUE" /><command name="glFoo" /></require>
                </extension>
              </extensions>
            </registry>
            """);
        var config = OpenGlRegistryTestConfig.Create();
        config.GlExtensions = ["GL_TEST_extension"];

        var model = new GlRegistryParser(config).Parse(registry, new Dictionary<string, XmlDocComment>());

        var command = model.Commands.Single();
        Assert.AreEqual("GL_TEST_extension", command.Availability.Gl);
        Assert.AreEqual("3.0", command.Availability.GlEs);
        Assert.AreEqual("GlFooGroup", command.Parameters.Single().ManagedType);
    }

    /// <summary>Handles, generic object identifiers, bool returns, and wide constants are mapped.</summary>
    [TestMethod]
    public void Parse_MapsHandlesBooleansObjectIdentifiersAndWideConstants()
    {
        using var workspace = TempWorkspace.Create();
        var registry = workspace.WriteFile("gl.xml", """
            <registry>
              <enums>
                <enum name="GL_TIMEOUT_IGNORED" value="0xFFFFFFFFFFFFFFFF" />
              </enums>
              <commands>
                <command>
                  <proto><ptype>GLboolean</ptype> <name>glIsBuffer</name></proto>
                  <param class="buffer"><ptype>GLuint</ptype> <name>buffer</name></param>
                </command>
                <command>
                  <proto>void <name>glDeleteObject</name></proto>
                  <param group="ObjectIdentifier"><ptype>GLuint</ptype> <name>name</name></param>
                </command>
              </commands>
              <feature api="gl" name="GL_VERSION_1_0" number="1.0">
                <require>
                  <enum name="GL_TIMEOUT_IGNORED" />
                  <command name="glIsBuffer" />
                  <command name="glDeleteObject" />
                </require>
              </feature>
            </registry>
            """);

        var model = new GlRegistryParser(OpenGlRegistryTestConfig.Create()).Parse(registry, new Dictionary<string, XmlDocComment>());

        var isBuffer = model.Commands.Single(command => command.NativeName == "glIsBuffer");
        Assert.AreEqual("bool", isBuffer.ReturnType);
        Assert.AreEqual("byte", isBuffer.ReturnInteropType);
        Assert.AreEqual("GlBufferHandle", isBuffer.Parameters.Single().ManagedType);
        Assert.AreEqual("GlHandle", model.Commands.Single(command => command.NativeName == "glDeleteObject").Parameters.Single().ManagedType);
        CollectionAssert.AreEquivalent(new[] { "GlBufferHandle", "GlHandle" }, model.HandleTypes.ToArray());
        Assert.AreEqual("TimeoutIgnored", model.WideConstants.Single().ManagedName);
    }
}
