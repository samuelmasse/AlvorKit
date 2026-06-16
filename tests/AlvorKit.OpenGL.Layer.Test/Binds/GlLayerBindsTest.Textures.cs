namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void BindTexture_ThenUnbind_Succeeds()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(1));
        gl.UnbindTexture(GlTextureTarget.Texture2D);
    }

    [TestMethod]
    public void BindTexture_WithoutActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.BindTexture(GlTextureTarget.Texture2D, Texture(1)));
    }

    [TestMethod]
    public void ActiveTexture_SetTwiceWithoutReset_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        Assert.Throws<GlAlreadySetException>(() => gl.ActiveTexture(GlTextureUnit.Texture1));
    }

    [TestMethod]
    public void ResetActiveTexture_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetActiveTexture());
    }

    [TestMethod]
    public void BindTexture_SameTextureDifferentTarget_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(1));
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        Assert.Throws<GlBindConflictException>(() => gl.BindTexture(GlTextureTarget.Texture3D, Texture(1)));
    }

    [TestMethod]
    public void BindTexture_SameTargetDifferentUnits_DoNotConflict()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(1));
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.ActiveTexture(GlTextureUnit.Texture1);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(2));
    }

    [TestMethod]
    public void BindTextureUnit_WithoutKnownTarget_Throws()
    {
        Assert.Throws<GlException>(() => gl.BindTextureUnit(0, Texture(42)));
    }

    /// <summary>Unbinding a texture unit with no recorded target bindings reports the missing bind.</summary>
    [TestMethod]
    public void UnbindTextureUnit_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindTextureUnit(0));
    }

    [TestMethod]
    public void BindTexture_ThenBindTextureUnitSameUnit_Throws()
    {
        TrackTextureTarget(Texture(42), GlTextureTarget.Texture2D);
        TrackTextureTarget(Texture(43), GlTextureTarget.Texture2D);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(42));

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTextureUnit(0, Texture(43)));
    }

    [TestMethod]
    public void BindTextureUnit_ThenBindTextureSameUnit_Throws()
    {
        TrackTextureTarget(Texture(42), GlTextureTarget.Texture2D);
        gl.BindTextureUnit(0, Texture(42));
        gl.ActiveTexture(GlTextureUnit.Texture0);

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTexture(GlTextureTarget.Texture2D, Texture(43)));
    }

    [TestMethod]
    public void BindTexture_ThenBindTextureUnitSameUnitDifferentTarget_Succeeds()
    {
        TrackTextureTarget(Texture(42), GlTextureTarget.Texture2D);
        TrackTextureTarget(Texture(43), GlTextureTarget.TextureCubeMap);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, Texture(42));

        gl.BindTextureUnit(0, Texture(43));
    }

    [TestMethod]
    public void BindTextures_ThenUnbind_Succeeds()
    {
        TrackTextureTarget(Texture(1), GlTextureTarget.Texture2D);
        TrackTextureTarget(Texture(2), GlTextureTarget.Texture2D);
        gl.BindTextures(0, [Texture(1), Texture(2)]);
        gl.UnbindTextures(0, 2);
    }

    [TestMethod]
    public void BindTextureUnit_ThenBindTexturesSameUnit_Throws()
    {
        TrackTextureTarget(Texture(1), GlTextureTarget.Texture2D);
        TrackTextureTarget(Texture(2), GlTextureTarget.Texture2D);
        gl.BindTextureUnit(0, Texture(1));

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTextures(0, [Texture(2)]));
    }
}
