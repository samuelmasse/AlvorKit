namespace AlvorKit.Script.Bindgen;

/// <summary>Plans span substitutions for combined OpenGL overloads.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlCombinedSpanPlanner(GlExtensionEmissionState state)
{
    /// <summary>Applies typed and generic span substitutions to a combined overload plan.</summary>
    public void Apply(GlCombinedOverloadPlan plan)
    {
        plan.ConfiguredSpanParams = state.Config.SpanParams
            .GetValueOrDefault(plan.Command.NativeName, [])
            .ToHashSet(StringComparer.Ordinal);
        for (var i = 0; i < plan.Parameters.Count; i++)
        {
            if (plan.Plans[i] != GlExtensionPlanKind.Keep)
                continue;
            var parameter = plan.Parameters[i];
            var len = state.ParseLen(plan.Command, parameter);
            if (parameter is { PointeeType: not null, PointeeIsChar: false })
                PlanTypedSpan(plan, i, parameter, len);
            else if (parameter is { PointerDepth: 1, PointeeType: null, PointeeIsChar: false })
                PlanGenericSpan(plan, i, parameter, len);
        }
    }

    /// <summary>Plans a typed pointer parameter as a span when registry length metadata is usable.</summary>
    private static void PlanTypedSpan(GlCombinedOverloadPlan plan, int index, GlParameter parameter, GlExtensionLenInfo len)
    {
        if (len.Kind is not (GlExtensionLenKind.ParamRef or GlExtensionLenKind.Literal or GlExtensionLenKind.CompSize))
            return;
        if (len is { Kind: GlExtensionLenKind.Literal, Divisor: 1 } && !parameter.PointeeIsConst)
            return;
        plan.Plans[index] = GlExtensionPlanKind.SpanTyped;
        plan.Arguments[index] = $"(nint){GlExtensionNames.Local(parameter)}Ptr";
        plan.SpannedPointers.Add(index);
        if (len.Kind == GlExtensionLenKind.ParamRef)
            plan.AddCountReference(len.ParamIndex, index, len.Divisor);
    }

    /// <summary>Plans a void pointer parameter as a generic span when size metadata is usable or configured.</summary>
    private static void PlanGenericSpan(GlCombinedOverloadPlan plan, int index, GlParameter parameter, GlExtensionLenInfo len)
    {
        if (len.Kind == GlExtensionLenKind.ParamRef && plan.Parameters[len.ParamIndex].ManagedType == "nint")
        {
            plan.Plans[index] = GlExtensionPlanKind.SpanGenericSized;
            plan.Arguments[index] = $"(nint){GlExtensionNames.Local(parameter)}Ptr";
            plan.Plans[len.ParamIndex] = GlExtensionPlanKind.Dropped;
            plan.Arguments[len.ParamIndex] = $"ByteLength<{GlExtensionNames.TypeParameter(plan.Command, index)}>({parameter.ManagedName})";
        }
        else if (plan.ConfiguredSpanParams.Contains(parameter.NativeName))
        {
            plan.Plans[index] = GlExtensionPlanKind.SpanGenericUnsized;
            plan.Arguments[index] = $"(nint){GlExtensionNames.Local(parameter)}Ptr";
        }
    }
}
