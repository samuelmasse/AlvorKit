namespace AlvorKit.Script.Bindgen;

/// <summary>Resolves inferred count parameters for combined OpenGL overload plans.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlCombinedCountResolver(GlExtensionEmissionState state)
{
    /// <summary>Drops count parameters only when every dependent pointer has a managed substitute.</summary>
    public void Apply(GlCombinedOverloadPlan plan)
    {
        foreach (var (count, references) in plan.ReferencesByCount)
        {
            if (!CanDropCount(plan, count))
            {
                RestorePointers(plan, references);
                continue;
            }

            var firstReference = references[0];
            var first = plan.Parameters[firstReference.Pointer];
            plan.Plans[count] = GlExtensionPlanKind.Dropped;
            plan.Arguments[count] = GlExtensionNames.CountExpression(
                plan.Parameters[count],
                firstReference.Divisor == 1 ? $"{first.ManagedName}.Length" : $"{first.ManagedName}.Length / {firstReference.Divisor}");
        }
    }

    /// <summary>Returns whether a count parameter can be inferred for every raw referrer.</summary>
    private bool CanDropCount(GlCombinedOverloadPlan plan, int count)
    {
        if (plan.Plans[count] != GlExtensionPlanKind.Keep)
            return false;
        var referrers = plan.Parameters
            .Select((parameter, index) => (parameter, index))
            .Where(candidate => state.ParseLen(plan.Command, candidate.parameter) is { Kind: GlExtensionLenKind.ParamRef } len && len.ParamIndex == count);
        return referrers.All(referrer => plan.SpannedPointers.Contains(referrer.index) || plan.Plans[referrer.index] == GlExtensionPlanKind.Dropped);
    }

    /// <summary>Restores pointer parameters when their shared count cannot be inferred completely.</summary>
    private static void RestorePointers(GlCombinedOverloadPlan plan, IEnumerable<GlCountReference> references)
    {
        foreach (var reference in references)
        {
            plan.Plans[reference.Pointer] = GlExtensionPlanKind.Keep;
            plan.SpannedPointers.Remove(reference.Pointer);
        }
    }
}
