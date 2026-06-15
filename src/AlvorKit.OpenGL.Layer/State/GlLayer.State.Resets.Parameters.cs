namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Layer: Restores <c>glClipControl</c> to <see cref="DefaultClipControl"/>.
    /// Must be paired with exactly one earlier call to <c>glClipControl</c>.
    /// </summary>
    public void ResetClipControl()
    {
        clipControl.Reset(nameof(ClipControl));
        base.ClipControl(DefaultClipControl.Origin, DefaultClipControl.Depth);
    }

    /// <summary>
    /// Layer: Restores <c>glHint</c> for <paramref name="target"/>.
    /// Must be paired with exactly one earlier call to <c>glHint</c> for the same target.
    /// </summary>
    public void ResetHint(GlHintTarget target)
    {
        hintMap.Reset(nameof(Hint), target);
        base.Hint(target, DefaultHint);
    }

    /// <summary>
    /// Layer: Restores <c>glMinSampleShading</c> to <see cref="DefaultMinSampleShading"/>.
    /// Must be paired with exactly one earlier call to <c>glMinSampleShading</c>.
    /// </summary>
    public void ResetMinSampleShading()
    {
        minSampleShading.Reset(nameof(MinSampleShading));
        base.MinSampleShading(DefaultMinSampleShading);
    }

    /// <summary>
    /// Layer: Restores <c>glSampleCoverage</c> to <see cref="DefaultSampleCoverage"/>.
    /// Must be paired with exactly one earlier call to <c>glSampleCoverage</c>.
    /// </summary>
    public void ResetSampleCoverage()
    {
        sampleCoverage.Reset(nameof(SampleCoverage));
        base.SampleCoverage(DefaultSampleCoverage.Value, DefaultSampleCoverage.Invert);
    }

    /// <summary>
    /// Layer: Restores <c>glSampleMaski</c> for word <paramref name="maskNumber"/>.
    /// Must be paired with exactly one earlier call to <c>glSampleMaski</c> for the same word.
    /// </summary>
    public void ResetSampleMask(uint maskNumber)
    {
        sampleMaskMap.Reset(nameof(SampleMaski), maskNumber);
        base.SampleMaski(maskNumber, DefaultSampleMask);
    }

    /// <summary>
    /// Layer: Restores <c>glPatchParameteri</c> for <paramref name="pname"/>.
    /// Must be paired with exactly one earlier call to <c>glPatchParameteri</c> for the same parameter.
    /// </summary>
    public void ResetPatchParameter(GlPatchParameterName pname)
    {
        patchParameterMap.Reset(nameof(PatchParameteri), pname);
        base.PatchParameteri(pname, DefaultPatchParameter(pname));
    }

    /// <summary>
    /// Layer: Restores <c>glPointParameterf</c> for <paramref name="pname"/>.
    /// Must be paired with one earlier call to <c>glPointParameterf</c> or <c>glPointParameteri</c>.
    /// </summary>
    public void ResetPointParameter(GlPointParameterName pname)
    {
        pointParameterMap.Reset(nameof(PointParameterf), pname);
        base.PointParameterf(pname, DefaultPointParameter(pname));
    }

    /// <summary>
    /// Layer: Restores <c>glPixelStorei</c> for <paramref name="pname"/>.
    /// Must be paired with one earlier call to <c>glPixelStorei</c> or <c>glPixelStoref</c>.
    /// </summary>
    public void ResetPixelStore(GlPixelStoreParameter pname)
    {
        pixelStoreMap.Reset(nameof(PixelStorei), pname);
        base.PixelStorei(pname, DefaultPixelStore(pname));
    }

    /// <summary>
    /// Layer: Restores <c>glReadBuffer</c> to <see cref="DefaultReadBuffer"/>.
    /// Must be paired with exactly one earlier call to <c>glReadBuffer</c>.
    /// </summary>
    public void ResetReadBuffer()
    {
        readBuffer.Reset(nameof(ReadBuffer));
        base.ReadBuffer(DefaultReadBuffer);
    }
}
