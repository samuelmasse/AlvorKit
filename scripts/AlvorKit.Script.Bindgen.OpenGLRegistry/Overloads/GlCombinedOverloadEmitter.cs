namespace AlvorKit.Script.Bindgen;

/// <summary>Emits combined overloads where every safe OpenGL parameter transform is applied at once.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlCombinedOverloadEmitter(GlExtensionEmissionState state) : IGlOverloadEmitter
{
    /// <summary>Appends a combined overload when registry metadata supports one.</summary>
    public void Append(StringBuilder output, GlCommand command)
    {
        var plan = new GlCombinedOverloadPlan(command);
        new GlCombinedStringPlanner(state).Apply(plan);
        new GlCombinedSpanPlanner(state).Apply(plan);
        new GlCombinedCountResolver(state).Apply(plan);
        if (!plan.HasPublicTransform())
            return;

        var signature = GlCombinedSignatureBuilder.Build(plan);
        if (!state.AddSignature(signature.Key))
            return;
        new GlCombinedBodyEmitter(state).Append(output, plan, signature);
    }
}
