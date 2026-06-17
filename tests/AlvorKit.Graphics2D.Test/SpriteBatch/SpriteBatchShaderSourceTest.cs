namespace AlvorKit.Graphics2D.Test;

/// <summary>Tests the built-in sprite batch shader source generation.</summary>
[TestClass]
public sealed class SpriteBatchShaderSourceTest
{
    /// <summary>The vertex shader source contains the expected position output.</summary>
    [TestMethod]
    public void Vert_ReturnsPositionShader()
    {
        var source = SpriteBatchShaderSource.Vert();

        StringAssert.Contains(source, "gl_Position");
    }

    /// <summary>The fragment shader emits sampler branches for every texture slot after the first two.</summary>
    [TestMethod]
    public void Frag_WithExtraTextureSlots_AddsSamplerBranches()
    {
        var source = SpriteBatchShaderSource.Frag(3);

        StringAssert.Contains(source, "texSamplers[2]");
    }

    /// <summary>The fragment shader omits extra sampler branches when only two slots are requested.</summary>
    [TestMethod]
    public void Frag_WithTwoTextureSlots_UsesOnlyBaseBranches()
    {
        var source = SpriteBatchShaderSource.Frag(2);

        Assert.IsFalse(source.Contains("index == 2", StringComparison.Ordinal));
    }
}
