namespace AlvorKit.Script.Bindgen;

/// <summary>Builds generated method signatures for combined OpenGL overload plans.</summary>
internal static class GlCombinedSignatureBuilder
{
    /// <summary>Builds rendered signature metadata and fills missing raw arguments.</summary>
    public static GlCombinedSignature Build(GlCombinedOverloadPlan plan)
    {
        var typeParameters = new List<string>();
        var signature = new List<string>();
        for (var i = 0; i < plan.Parameters.Count; i++)
            AddParameter(plan, i, typeParameters, signature);

        var generics = typeParameters.Count > 0 ? $"<{string.Join(", ", typeParameters)}>" : "";
        var constraints = string.Concat(typeParameters.Select(typeParameter => $" where {typeParameter} : unmanaged"));
        var parameterList = string.Join(", ", signature);
        return new($"{plan.Command.ManagedName}{generics}({parameterList})", generics, parameterList, constraints);
    }

    /// <summary>Adds one public parameter or fills one raw call argument for a planned parameter.</summary>
    private static void AddParameter(
        GlCombinedOverloadPlan plan,
        int index,
        ICollection<string> typeParameters,
        ICollection<string> signature)
    {
        var parameter = plan.Parameters[index];
        switch (plan.Plans[index])
        {
            case GlExtensionPlanKind.Keep:
                signature.Add($"{parameter.ManagedType} {parameter.ManagedName}");
                plan.Arguments[index] = parameter.ManagedName;
                break;
            case GlExtensionPlanKind.SpanTyped:
                signature.Add($"{GlExtensionNames.SpanType(parameter, parameter.PointeeType!)} {parameter.ManagedName}");
                break;
            case GlExtensionPlanKind.SpanGenericSized or GlExtensionPlanKind.SpanGenericUnsized:
                var typeParameter = GlExtensionNames.TypeParameter(plan.Command, index);
                typeParameters.Add(typeParameter);
                signature.Add($"{GlExtensionNames.SpanType(parameter, typeParameter)} {parameter.ManagedName}");
                break;
            case GlExtensionPlanKind.StringIn:
                signature.Add($"string {parameter.ManagedName}");
                break;
            case GlExtensionPlanKind.StringArray:
                signature.Add($"ReadOnlySpan<string> {parameter.ManagedName}");
                break;
        }
    }
}
