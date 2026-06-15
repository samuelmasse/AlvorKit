namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTransformFeedback"/> for the same target.</remarks>
    public override void BindTransformFeedback(GlBindTransformFeedbackTarget target, GlTransformFeedbackHandle id)
    {
        transformFeedbackObject.Bind(nameof(BindTransformFeedback), (uint)id);
        base.BindTransformFeedback(target, id);
    }

    /// <summary>
    /// Layer: Returns <paramref name="target"/> to the default transform-feedback object.
    /// Must be paired with exactly one earlier call to <c>glBindTransformFeedback</c>.
    /// </summary>
    public void UnbindTransformFeedback(GlBindTransformFeedbackTarget target)
    {
        transformFeedbackObject.Unbind(nameof(BindTransformFeedback));
        base.BindTransformFeedback(target, (GlTransformFeedbackHandle)0u);
    }
}
