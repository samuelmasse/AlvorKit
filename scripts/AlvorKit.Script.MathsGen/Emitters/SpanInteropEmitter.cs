namespace AlvorKit.Script.MathsGen;

/// <summary>Emits span and array transfer helpers for generated vectors.</summary>
internal static class SpanInteropEmitter
{
    /// <summary>Appends constructors and copy helpers for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members) =>
        members.Append(MathsTemplate.Fragment("span-interop.csfrag.tmpl",
            ("TypeName", vector.TypeName),
            ("ScalarType", vector.Scalar.CSharpName),
            ("ConstructorArguments", ConstructorArguments(vector)),
            ("CopyAssignments", CopyAssignments(vector))));

    /// <summary>Returns span-backed component constructor arguments.</summary>
    private static string ConstructorArguments(VectorSpec vector)
    {
        var arguments = Enumerable.Range(0, vector.Dimension)
            .Select(index => $"ComponentFromSpan(values, {index.ToString(CultureInfo.InvariantCulture)})");

        return string.Join(", ", arguments);
    }

    /// <summary>Returns statements that copy each component into a destination span.</summary>
    private static string CopyAssignments(VectorSpec vector)
    {
        var assignments = vector.Components.Select((component, index) =>
            $"        destination[{index.ToString(CultureInfo.InvariantCulture)}] = {component};");

        return string.Join(Environment.NewLine, assignments);
    }
}
