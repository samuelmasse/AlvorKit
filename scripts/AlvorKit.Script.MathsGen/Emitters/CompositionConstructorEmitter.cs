namespace AlvorKit.Script.MathsGen;

/// <summary>Emits GLM-style constructors that compose vectors from smaller vectors.</summary>
internal static class CompositionConstructorEmitter
{
    /// <summary>Appends vector composition constructors for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        if (vector.Dimension == 2)
            EmitTruncatingConstructors(vector, members);
        else if (vector.Dimension == 3)
            EmitVec3(vector, members);
        else
            EmitVec4(vector, members);
    }

    /// <summary>Emits constructors from higher-dimension vectors.</summary>
    private static void EmitTruncatingConstructors(VectorSpec target, MemberBlock members)
    {
        foreach (var source in VectorCatalog.Vectors.Where(source => source.Dimension > target.Dimension))
            Add(members, $"Creates a {target.TypeName} from the first {target.Dimension} components of {source.TypeName}.", target.TypeName,
                $"{source.TypeName} value", Args(target, Cast(target, source.Scalar, target.Components.Select(c => $"value.{c}"))));
    }

    /// <summary>Emits 3D vector composition constructors.</summary>
    private static void EmitVec3(VectorSpec target, MemberBlock members)
    {
        EmitTruncatingConstructors(target, members);
        foreach (var source in VectorCatalog.Scalars)
        {
            Add(members, $"Creates a {target.TypeName} from an XY vector and a Z component.", target.TypeName,
                $"{source.VectorName(2)} xy, {target.Scalar.CSharpName} z", Args(target, [.. Cast(target, source, ["xy.X", "xy.Y"]), "z"]));
            Add(members, $"Creates a {target.TypeName} from an X component and a YZ vector.", target.TypeName,
                $"{target.Scalar.CSharpName} x, {source.VectorName(2)} yz", Args(target, ["x", .. Cast(target, source, ["yz.X", "yz.Y"])]));
        }
    }

    /// <summary>Emits 4D vector composition constructors.</summary>
    private static void EmitVec4(VectorSpec target, MemberBlock members)
    {
        foreach (var source in VectorCatalog.Scalars)
        {
            Add(members, $"Creates a {target.TypeName} from an XY vector and ZW components.", target.TypeName,
                $"{source.VectorName(2)} xy, {target.Scalar.CSharpName} z, {target.Scalar.CSharpName} w",
                Args(target, [.. Cast(target, source, ["xy.X", "xy.Y"]), "z", "w"]));
            Add(members, $"Creates a {target.TypeName} from X and W components around a YZ vector.", target.TypeName,
                $"{target.Scalar.CSharpName} x, {source.VectorName(2)} yz, {target.Scalar.CSharpName} w",
                Args(target, ["x", .. Cast(target, source, ["yz.X", "yz.Y"]), "w"]));
            Add(members, $"Creates a {target.TypeName} from XY components and a ZW vector.", target.TypeName,
                $"{target.Scalar.CSharpName} x, {target.Scalar.CSharpName} y, {source.VectorName(2)} zw",
                Args(target, ["x", "y", .. Cast(target, source, ["zw.X", "zw.Y"])]));
            Add(members, $"Creates a {target.TypeName} from an XYZ vector and a W component.", target.TypeName,
                $"{source.VectorName(3)} xyz, {target.Scalar.CSharpName} w",
                Args(target, [.. Cast(target, source, ["xyz.X", "xyz.Y", "xyz.Z"]), "w"]));
            Add(members, $"Creates a {target.TypeName} from an X component and a YZW vector.", target.TypeName,
                $"{target.Scalar.CSharpName} x, {source.VectorName(3)} yzw",
                Args(target, ["x", .. Cast(target, source, ["yzw.X", "yzw.Y", "yzw.Z"])]));
        }

        foreach (var left in VectorCatalog.Scalars)
        {
            foreach (var right in VectorCatalog.Scalars)
            {
                Add(members, $"Creates a {target.TypeName} from XY and ZW vectors.", target.TypeName,
                    $"{left.VectorName(2)} xy, {right.VectorName(2)} zw",
                    Args(target, [.. Cast(target, left, ["xy.X", "xy.Y"]), .. Cast(target, right, ["zw.X", "zw.Y"])]));
            }
        }
    }

    /// <summary>Renders and appends one constructor.</summary>
    private static void Add(MemberBlock members, string summary, string typeName, string parameters, string arguments) =>
        members.Append(MathsTemplate.Fragment("composition-constructor.csfrag.tmpl", ("Summary", summary), ("TypeName", typeName),
            ("Parameters", parameters), ("Arguments", arguments)));

    /// <summary>Returns constructor arguments from already-converted expressions.</summary>
    private static string Args(VectorSpec target, IEnumerable<string> expressions)
    {
        var arguments = expressions.Select(expression => target.Scalar.CastArithmetic(expression)).ToArray();
        var inline = string.Join(", ", arguments);

        if (inline.Length <= 120)
            return inline;

        var newline = Environment.NewLine;
        return $"{newline}            {string.Join($",{newline}            ", arguments)}{newline}        ";
    }

    /// <summary>Returns converted vector component expressions.</summary>
    private static string[] Cast(VectorSpec target, ScalarSpec source, IEnumerable<string> expressions) =>
        [.. expressions.Select(expression => ConversionEmitter.CastComponent(target.Scalar, source, expression))];
}
