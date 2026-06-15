namespace AlvorKit.Script.Bindgen;

/// <summary>Plans string and string-array substitutions for combined OpenGL overloads.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlCombinedStringPlanner(GlExtensionEmissionState state)
{
    /// <summary>Applies string substitutions to a combined overload plan.</summary>
    public void Apply(GlCombinedOverloadPlan plan)
    {
        PlanStringInputs(plan);
        PlanStringArrays(plan);
    }

    /// <summary>Plans const GLchar pointer parameters as managed strings.</summary>
    private void PlanStringInputs(GlCombinedOverloadPlan plan)
    {
        for (var i = 0; i < plan.Parameters.Count; i++)
        {
            var parameter = plan.Parameters[i];
            if (parameter is not { PointerDepth: 1, PointeeIsChar: true, PointeeIsConst: true })
                continue;
            plan.Plans[i] = GlExtensionPlanKind.StringIn;
            plan.Arguments[i] = $"{GlExtensionNames.Local(parameter)}Utf8.Pointer";
            foreach (var lengthArg in state.ParseLen(plan.Command, parameter) is { Kind: GlExtensionLenKind.CompSize } len ? len.CompSizeArgs : [])
                DropStringLengthArgument(plan, parameter, lengthArg);
        }
    }

    /// <summary>Drops a count argument paired with a string input when it can be inferred safely.</summary>
    private static void DropStringLengthArgument(GlCombinedOverloadPlan plan, GlParameter parameter, string lengthArg)
    {
        var paired = plan.Parameters.ToList()
            .FindIndex(candidate => candidate.NativeName == lengthArg && candidate is { PointerDepth: 0, ManagedType: "int" or "uint" });
        if (paired < 0)
            return;
        plan.Plans[paired] = GlExtensionPlanKind.Dropped;
        plan.Arguments[paired] = GlExtensionNames.CountExpression(plan.Parameters[paired], $"{GlExtensionNames.Local(parameter)}Utf8.Length");
    }

    /// <summary>Plans const GLchar pointer arrays as managed spans of strings.</summary>
    private void PlanStringArrays(GlCombinedOverloadPlan plan)
    {
        for (var i = 0; i < plan.Parameters.Count; i++)
        {
            var parameter = plan.Parameters[i];
            if (parameter is not { PointerDepth: 2, PointeeIsChar: true, PointeeIsConst: true })
                continue;
            var countIndex = CountIndex(plan, parameter);
            if (countIndex < 0)
                continue;
            plan.Plans[i] = GlExtensionPlanKind.StringArray;
            plan.Arguments[i] = $"{GlExtensionNames.Local(parameter)}Array.Pointers";
            plan.SpannedPointers.Add(i);
            plan.AddCountReference(countIndex, i, 1);
            DropLengthArray(plan, countIndex);
        }
    }

    /// <summary>Finds the parameter index that controls a string array count.</summary>
    private int CountIndex(GlCombinedOverloadPlan plan, GlParameter parameter)
    {
        var len = state.ParseLen(plan.Command, parameter);
        if (len.Kind == GlExtensionLenKind.ParamRef)
            return len.ParamIndex;
        return len.Kind == GlExtensionLenKind.CompSize && len.CompSizeArgs.Length == 1
            ? plan.Parameters.ToList().FindIndex(candidate =>
                candidate.NativeName == len.CompSizeArgs[0]
                && candidate is { PointerDepth: 0, ManagedType: "int" or "uint" })
            : -1;
    }

    /// <summary>Drops an optional per-string length array after strings are NUL-terminated.</summary>
    private void DropLengthArray(GlCombinedOverloadPlan plan, int countIndex)
    {
        for (var j = 0; j < plan.Parameters.Count; j++)
            if (plan.Parameters[j] is { PointerDepth: 1, PointeeType: "int", PointeeIsConst: true }
                && state.ParseLen(plan.Command, plan.Parameters[j]) is { Kind: GlExtensionLenKind.ParamRef } len
                && len.ParamIndex == countIndex)
            {
                plan.Plans[j] = GlExtensionPlanKind.Dropped;
                plan.Arguments[j] = "0";
            }
    }
}
