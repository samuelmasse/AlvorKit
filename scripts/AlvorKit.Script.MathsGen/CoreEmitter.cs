namespace AlvorKit.Script.MathsGen;

/// <summary>Emits fields, constructors, constants, indexing, and deconstruction.</summary>
internal static class CoreEmitter
{
    /// <summary>Appends core vector members to <paramref name="members"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        members.Append(MathsTemplate.Fragment("const-int.csfrag.tmpl", ("Summary", $"The number of scalar components in a {vector.TypeName}."),
            ("Name", "ComponentCount"), ("Value", vector.Dimension.ToString(CultureInfo.InvariantCulture))));
        members.Append(MathsTemplate.Fragment("const-int.csfrag.tmpl", ("Summary", $"The byte size of a {vector.TypeName}."),
            ("Name", "SizeInBytes"), ("Value", (vector.Dimension * vector.Scalar.SizeBytes).ToString(CultureInfo.InvariantCulture))));
        if (vector.Scalar.IsBool)
            members.Append(MathsTemplate.Fragment("private-const-int.csfrag.tmpl", ("Summary", "The byte distance between Boolean components."),
                ("Name", "ComponentStrideBytes"), ("Value", "4")));

        EmitFields(vector, members);
        members.Append(MathsTemplate.Fragment("scalar-constructor.csfrag.tmpl", ("TypeName", vector.TypeName),
            ("ScalarType", vector.Scalar.CSharpName), ("Arguments", string.Join(", ", Enumerable.Repeat("value", vector.Dimension)))));
        EmitFactories(vector, members);
        EmitConstants(vector, members);
        EmitExplicitInterfaceProperties(vector, members);
        members.Append(MathsTemplate.Fragment(vector.Scalar.IsBool ? "indexer-bool.csfrag.tmpl" : "indexer-numeric.csfrag.tmpl",
            ("TypeName", vector.TypeName), ("ScalarType", vector.Scalar.CSharpName),
            ("MaxIndex", (vector.Dimension - 1).ToString(CultureInfo.InvariantCulture))));
        members.Append(MathsTemplate.Fragment("deconstruct.csfrag.tmpl", ("Parameters", DeconstructParameters(vector)),
            ("Assignments", DeconstructAssignments(vector))));
    }

    /// <summary>Emits primary coordinate fields and their color/texture aliases.</summary>
    private static void EmitFields(VectorSpec vector, MemberBlock members)
    {
        EmitFieldGroup(vector, members, VectorCatalog.Components, ["coordinate"], initialize: true);
        EmitFieldGroup(vector, members, ["R", "G", "B", "A"], ["color alias"], initialize: false);
        EmitFieldGroup(vector, members, ["S", "T", "P", "Q"], ["texture-coordinate alias"], initialize: false);
    }

    /// <summary>Emits one alias group of fields.</summary>
    private static void EmitFieldGroup(VectorSpec vector, MemberBlock members, string[] names, string[] descriptions, bool initialize)
    {
        for (var index = 0; index < vector.Dimension; index++)
        {
            var name = names[index];
            var description = descriptions[0];
            var summary = initialize ? $"The {name} {description} component." : $"The {name} {description} for {vector.Components[index]}.";
            var initializer = initialize ? $" = {vector.Parameters[index]}" : "";
            members.Append(MathsTemplate.Fragment("field.csfrag.tmpl", ("Summary", summary), ("Offset", Offset(vector, index)),
                ("ScalarType", vector.Scalar.CSharpName), ("Name", name), ("Initializer", initializer)));
        }
    }

    /// <summary>Emits static factories used by vector interfaces and ordinary callers.</summary>
    private static void EmitFactories(VectorSpec vector, MemberBlock members)
    {
        var scalar = vector.Scalar.CSharpName;
        members.Append(NumericFunctionsEmitter.Method("Creates a vector with every component set to value.", "static",
            vector.TypeName, "Create", $"{scalar} value", "new(value)"));
        members.Append(NumericFunctionsEmitter.Method("Creates a vector from scalar components.", "static",
            vector.TypeName, "Create", ComponentFactoryParameters(vector), $"new({string.Join(", ", vector.Parameters)})"));
    }

    /// <summary>Emits explicit static properties needed by static-abstract interfaces.</summary>
    private static void EmitExplicitInterfaceProperties(VectorSpec vector, MemberBlock members)
    {
        var vectorInterface = $"IVec<{vector.TypeName}, {vector.Scalar.CSharpName}>";
        members.Append(ExplicitProperty("Gets the number of scalar components.", "int", vectorInterface,
            "ComponentCount", "ComponentCount"));
        members.Append(ExplicitProperty("Gets the byte size.", "int", vectorInterface,
            "SizeInBytes", "SizeInBytes"));
        if (vector.Scalar.IsBool)
            return;

        members.Append(ExplicitProperty("Gets the additive identity.", vector.TypeName,
            $"IAdditiveIdentity<{vector.TypeName}, {vector.TypeName}>", "AdditiveIdentity", "Zero"));
        members.Append(ExplicitProperty("Gets the multiplicative identity.", vector.TypeName,
            $"IMultiplicativeIdentity<{vector.TypeName}, {vector.TypeName}>", "MultiplicativeIdentity", "One"));
    }

    /// <summary>Emits common zero/one/unit constants or Boolean aggregate constants.</summary>
    private static void EmitConstants(VectorSpec vector, MemberBlock members)
    {
        if (vector.Scalar.IsBool)
        {
            members.Append(Property("Gets a mask with every component set to false.", "static", vector.TypeName, "False", "default"));
            members.Append(Property("Gets a mask with every component set to true.", "static", vector.TypeName, "True", "new(true)"));
            return;
        }

        members.Append(Property("Gets the zero vector.", "static", vector.TypeName, "Zero", "default"));
        members.Append(Property("Gets a vector with every component set to one.", "static", vector.TypeName, "One", $"new({vector.Scalar.OneLiteral})"));
        if (vector.Scalar.IsFloating)
            EmitFloatingConstants(vector, members);

        for (var index = 0; index < vector.Dimension; index++)
        {
            var args = Enumerable.Range(0, vector.Dimension).Select(i => i == index ? vector.Scalar.OneLiteral : vector.Scalar.ZeroLiteral);
            members.Append(Property($"Gets the unit vector pointing along the positive {vector.Components[index]} axis.", "static",
                vector.TypeName, $"Unit{vector.Components[index]}", $"new({string.Join(", ", args)})"));
        }
    }

    /// <summary>Emits floating-point special-value constants.</summary>
    private static void EmitFloatingConstants(VectorSpec vector, MemberBlock members)
    {
        members.Append(Property("Gets a vector with every component set to positive infinity.", "static", vector.TypeName,
            "PositiveInfinity", $"new({vector.Scalar.CSharpName}.PositiveInfinity)"));
        members.Append(Property("Gets a vector with every component set to negative infinity.", "static", vector.TypeName,
            "NegativeInfinity", $"new({vector.Scalar.CSharpName}.NegativeInfinity)"));
        members.Append(Property("Gets a vector with every component set to NaN.", "static", vector.TypeName,
            "NaN", $"new({vector.Scalar.CSharpName}.NaN)"));
        members.Append(Property("Gets a vector with every component set to the smallest positive scalar value.", "static", vector.TypeName,
            "Epsilon", $"new({vector.Scalar.CSharpName}.Epsilon)"));
    }

    /// <summary>Renders a simple generated property.</summary>
    private static string Property(string summary, string modifiers, string type, string name, string expression) =>
        MathsTemplate.Fragment("property-expression.csfrag.tmpl", ("Summary", summary), ("Modifiers", modifiers), ("Type", type),
            ("Name", name), ("Expression", expression));

    /// <summary>Renders an explicit interface property implementation.</summary>
    private static string ExplicitProperty(string summary, string type, string interfaceName, string name, string expression) =>
        MathsTemplate.Fragment("explicit-interface-property.csfrag.tmpl", ("Summary", summary), ("Type", type),
            ("Interface", interfaceName), ("Name", name), ("Expression", expression));

    /// <summary>Returns the byte offset for a component index.</summary>
    private static string Offset(VectorSpec vector, int index) => (index * vector.Scalar.SizeBytes).ToString(CultureInfo.InvariantCulture);

    /// <summary>Returns deconstruction parameter text.</summary>
    private static string DeconstructParameters(VectorSpec vector) =>
        string.Join(", ", vector.Parameters.Select(parameter => $"out {vector.Scalar.CSharpName} {parameter}"));

    /// <summary>Returns static factory parameter text.</summary>
    private static string ComponentFactoryParameters(VectorSpec vector) =>
        string.Join(", ", vector.Parameters.Select(parameter => $"{vector.Scalar.CSharpName} {parameter}"));

    /// <summary>Returns deconstruction assignment statements.</summary>
    private static string DeconstructAssignments(VectorSpec vector) =>
        string.Join(Environment.NewLine, vector.Components.Zip(vector.Parameters, (component, parameter) => $"        {parameter} = {component};"));
}
