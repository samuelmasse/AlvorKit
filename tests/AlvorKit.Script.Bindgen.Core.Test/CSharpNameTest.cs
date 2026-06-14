using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

[TestClass]
public sealed class CSharpNameTest
{
    [TestMethod]
    public void FromNativeIdentifier_StripsPrefixesAndKeepsReadableDigitSegments()
    {
        Assert.AreEqual("GetWindowSize", CSharpName.FromNativeIdentifier("glfwGetWindowSize", "glfw"));
        Assert.AreEqual("Texture2D", CSharpName.FromNativeIdentifier("GL_TEXTURE_2D", "GL_", dimensionSegments: true));
        Assert.AreEqual("Xxh3_64bits", CSharpName.FromNativeIdentifier("XXH3_64bits", "XXH", "Xxh"));
        Assert.AreEqual("D0", CSharpName.FromNativeIdentifier("GLFW_KEY_0", "GLFW_KEY_", "D"));
    }

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
    }
}
