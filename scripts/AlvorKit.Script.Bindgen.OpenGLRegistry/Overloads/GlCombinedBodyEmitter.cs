namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated method bodies for combined OpenGL overload plans.</summary>
/// <param name="state">Shared extension-emission state.</param>
internal sealed class GlCombinedBodyEmitter(GlExtensionEmissionState state)
{
    /// <summary>Appends one planned combined overload body.</summary>
    public void Append(StringBuilder output, GlCombinedOverloadPlan plan, GlCombinedSignature signature)
    {
        GlExtensionDocEmitter.Emit(output, state.Config, plan.Command, Detail(plan));
        var declaration =
            $"    public virtual {plan.Command.ReturnType} {plan.Command.ManagedName}{signature.GenericSuffix}" +
            $"({signature.ParameterList}){signature.Constraints}";
        output.AppendLine(declaration);
        output.AppendLine("    {");
        EmitStringLocals(output, plan);
        var fixedCount = EmitFixedStatements(output, plan);
        var call = $"this.{plan.Command.ManagedName}({string.Join(", ", plan.Arguments.Where(value => value is not null))})";
        output.AppendLine($"        {(fixedCount > 0 ? "    " : "")}{(plan.Command.ReturnType == "void" ? call : "return " + call)};");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>Builds the XML documentation detail sentence for a planned overload.</summary>
    private static string Detail(GlCombinedOverloadPlan plan)
    {
        var pinnedSpans = new List<string>();
        var pinnedStrings = new List<string>();
        var stringArrays = new List<string>();
        var dropped = new List<string>();
        for (var i = 0; i < plan.Parameters.Count; i++)
            AddDetailBucket(plan, i, pinnedSpans, pinnedStrings, stringArrays, dropped);

        var detailParts = new List<string>();
        if (pinnedSpans.Count > 0)
            detailParts.Add($"Pins {GlExtensionNames.ParamRefs(pinnedSpans)} for the duration of the call.");
        if (pinnedStrings.Count > 0)
            detailParts.Add($"Marshals {GlExtensionNames.ParamRefs(pinnedStrings)} to NUL-terminated UTF-8.");
        if (stringArrays.Count > 0)
            detailParts.Add($"Marshals {GlExtensionNames.ParamRefs(stringArrays)} into a NUL-terminated UTF-8 string array.");
        if (dropped.Count > 0)
            detailParts.Add($"Supplies {GlExtensionNames.CodeNames(dropped)} automatically from the span length{(dropped.Count > 1 ? "s" : "")}.");
        return string.Join(" ", detailParts);
    }

    /// <summary>Adds one parameter to the relevant documentation detail bucket.</summary>
    private static void AddDetailBucket(
        GlCombinedOverloadPlan plan,
        int index,
        ICollection<string> pinnedSpans,
        ICollection<string> pinnedStrings,
        ICollection<string> stringArrays,
        ICollection<string> dropped)
    {
        var name = plan.Parameters[index].ManagedName;
        if (plan.Plans[index] is GlExtensionPlanKind.SpanTyped or GlExtensionPlanKind.SpanGenericSized or GlExtensionPlanKind.SpanGenericUnsized)
            pinnedSpans.Add(name);
        else if (plan.Plans[index] == GlExtensionPlanKind.StringIn)
            pinnedStrings.Add(name);
        else if (plan.Plans[index] == GlExtensionPlanKind.StringArray)
            stringArrays.Add(name);
        else if (plan.Plans[index] == GlExtensionPlanKind.Dropped)
            dropped.Add(name);
    }

    /// <summary>Emits UTF-8 helper locals for planned string parameters.</summary>
    private static void EmitStringLocals(StringBuilder output, GlCombinedOverloadPlan plan)
    {
        for (var i = 0; i < plan.Parameters.Count; i++)
        {
            var parameter = plan.Parameters[i];
            var local = GlExtensionNames.Local(parameter);
            if (plan.Plans[i] == GlExtensionPlanKind.StringIn)
                output.AppendLine($"        using var {local}Utf8 = new Utf8({parameter.ManagedName}, stackalloc byte[256]);");
            else if (plan.Plans[i] == GlExtensionPlanKind.StringArray)
                output.AppendLine($"        using var {local}Array = new Utf8Array({parameter.ManagedName}, stackalloc nint[128]);");
        }
    }

    /// <summary>Emits fixed statements for planned span parameters and returns how many were emitted.</summary>
    private static int EmitFixedStatements(StringBuilder output, GlCombinedOverloadPlan plan)
    {
        var fixedCount = 0;
        for (var i = 0; i < plan.Parameters.Count; i++)
        {
            var parameter = plan.Parameters[i];
            var local = GlExtensionNames.Local(parameter);
            if (plan.Plans[i] == GlExtensionPlanKind.SpanTyped)
                output.AppendLine($"        fixed ({parameter.PointeeType}* {local}Ptr = {parameter.ManagedName})");
            else if (plan.Plans[i] is GlExtensionPlanKind.SpanGenericSized or GlExtensionPlanKind.SpanGenericUnsized)
            {
                var typeParameter = GlExtensionNames.TypeParameter(plan.Command, i);
                output.AppendLine($"        fixed ({typeParameter}* {local}Ptr = {parameter.ManagedName})");
            }
            else
                continue;
            fixedCount++;
        }
        return fixedCount;
    }
}
