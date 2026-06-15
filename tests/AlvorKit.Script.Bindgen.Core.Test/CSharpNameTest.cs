namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers native-to-managed C# naming rules.</summary>
[TestClass]
public sealed class CSharpNameTest
{
    /// <summary>Prefix stripping and digit segments produce readable PascalCase names.</summary>
    [TestMethod]
    public void FromNativeIdentifier_StripsPrefixesAndKeepsReadableDigitSegments()
    {
        Assert.AreEqual("GetWindowSize", CSharpName.FromNativeIdentifier("glfwGetWindowSize", "glfw"));
        Assert.AreEqual("Texture2D", CSharpName.FromNativeIdentifier("GL_TEXTURE_2D", "GL_", dimensionSegments: true));
        Assert.AreEqual("Xxh3_64bits", CSharpName.FromNativeIdentifier("XXH3_64bits", "XXH", "Xxh"));
        Assert.AreEqual("D0", CSharpName.FromNativeIdentifier("GLFW_KEY_0", "GLFW_KEY_", "D"));
    }

    /// <summary>Known native acronym tokens keep conventional managed casing.</summary>
    [TestMethod]
    public void FromNativeIdentifier_PreservesKnownAcronymCasing()
    {
        Assert.AreEqual("OpenGLESApi", CSharpName.FromNativeIdentifier("GLFW_OPENGL_ES_API", "GLFW_"));
        Assert.AreEqual("GamepadButtonDPadUp", CSharpName.FromNativeIdentifier("GLFW_GAMEPAD_BUTTON_DPAD_UP", "GLFW_"));
        Assert.AreEqual("Win32ShowDefault", CSharpName.FromNativeIdentifier("GLFW_WIN32_SHOWDEFAULT", "GLFW_"));
        Assert.AreEqual("X11XCBVulkanSurface", CSharpName.FromNativeIdentifier("GLFW_X11_XCB_VULKAN_SURFACE", "GLFW_"));
    }

    /// <summary>Native type names add the configured managed type prefix after normal conversion.</summary>
    [TestMethod]
    public void FromNativeTypeName_AddsManagedTypePrefix()
    {
        Assert.AreEqual("FtFace", CSharpName.FromNativeTypeName("FT_Face", "FT_", "Ft"));
    }

    /// <summary>Empty native identifiers are rejected before the converter can index into missing text.</summary>
    [TestMethod]
    public void FromNativeIdentifier_RejectsEmptyNativeNames()
    {
        Assert.ThrowsException<ArgumentException>(() => CSharpName.FromNativeIdentifier("", "GL_"));
        Assert.ThrowsException<ArgumentException>(() => CSharpName.FromNativeIdentifier("___", "GL_"));
    }

    /// <summary>Keyword escaping only changes names that would collide with C# syntax.</summary>
    [TestMethod]
    public void Parameter_EscapesEveryCSharpKeyword()
    {
        Assert.AreEqual("@string", CSharpName.Parameter("string"));
        Assert.AreEqual("@delegate", CSharpName.Parameter("delegate"));
        Assert.AreEqual("@namespace", CSharpName.Parameter("namespace"));
        Assert.AreEqual("@record", CSharpName.Parameter("record"));
        Assert.AreEqual("@await", CSharpName.Parameter("await"));
        Assert.AreEqual("@where", CSharpName.Parameter("where"));
        Assert.AreEqual("@value", CSharpName.Parameter("value"));
        Assert.AreEqual("ordinary", CSharpName.Parameter("ordinary"));
    }
}
