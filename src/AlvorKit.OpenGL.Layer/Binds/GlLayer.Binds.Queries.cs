namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQuery</c> for the same target.</remarks>
    public override void BeginQuery(GlQueryTarget target, GlQueryHandle id)
    {
        queryBinds.Bind(nameof(BeginQuery), target, (uint)id);
        base.BeginQuery(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQuery</c> for the same target.</remarks>
    public override void EndQuery(GlQueryTarget target)
    {
        queryBinds.Unbind(nameof(EndQuery), target);
        base.EndQuery(target);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQueryIndexed</c> for the same target and index.</remarks>
    public override void BeginQueryIndexed(GlQueryTarget target, uint index, GlQueryHandle id)
    {
        queryIndexedBinds.Bind(nameof(BeginQueryIndexed), (target, index), (uint)id);
        base.BeginQueryIndexed(target, index, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQueryIndexed</c> for the same target and index.</remarks>
    public override void EndQueryIndexed(GlQueryTarget target, uint index)
    {
        queryIndexedBinds.Unbind(nameof(EndQueryIndexed), (target, index));
        base.EndQueryIndexed(target, index);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndConditionalRender</c>.</remarks>
    public override void BeginConditionalRender(uint id, GlConditionalRenderMode mode)
    {
        conditionalRender.Bind(nameof(BeginConditionalRender), id);
        base.BeginConditionalRender(id, mode);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginConditionalRender</c>.</remarks>
    public override void EndConditionalRender()
    {
        conditionalRender.Unbind(nameof(EndConditionalRender));
        base.EndConditionalRender();
    }
}
