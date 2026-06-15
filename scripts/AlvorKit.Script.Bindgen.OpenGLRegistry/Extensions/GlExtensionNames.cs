namespace AlvorKit.Script.Bindgen;

/// <summary>Formats generated names and small expressions for OpenGL extension overloads.</summary>
internal static class GlExtensionNames
{
    /// <summary>Local variable name for generated unsafe code, without keyword escaping.</summary>
    public static string Local(GlParameter parameter) => parameter.ManagedName.TrimStart('@');

    /// <summary>Fully qualified cref for the raw command, avoiding ambiguity with overloads.</summary>
    public static string CoreCref(BindgenConfig config, GlCommand command) =>
        $"{config.ApiClass}.{command.ManagedName}({string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedType))})";

    /// <summary>Renders overload parameter names as paramref references.</summary>
    public static string ParamRefs(IEnumerable<string> names) =>
        string.Join(", ", names.Select(name => $"<paramref name=\"{name.TrimStart('@')}\"/>"));

    /// <summary>Renders inferred raw argument names in code font because they are not overload parameters.</summary>
    public static string CodeNames(IEnumerable<string> names) =>
        string.Join(", ", names.Select(name => $"<c>{name.TrimStart('@')}</c>"));

    /// <summary>Formats a span type based on whether the native pointer is const.</summary>
    public static string SpanType(GlParameter parameter, string elementType) =>
        $"{(parameter.PointeeIsConst ? "ReadOnlySpan" : "Span")}<{elementType}>";

    /// <summary>Formats a generic type parameter for a void-pointer span overload.</summary>
    public static string TypeParameter(GlCommand command, int pointerIndex)
    {
        var generic = command.Parameters
            .Where(parameter => parameter is { PointerDepth: 1, PointeeType: null, PointeeIsChar: false })
            .ToList();
        if (generic.Count == 1)
            return "T";
        var name = Local(command.Parameters[pointerIndex]);
        return "T" + char.ToUpperInvariant(name[0]) + name[1..];
    }

    /// <summary>Casts inferred count expressions to the native count parameter type when needed.</summary>
    public static string CountExpression(GlParameter count, string expression) =>
        count.ManagedType == "int" ? expression : $"({count.ManagedType})({expression})";

    /// <summary>Converts a plural generated name to its singular helper form.</summary>
    public static string Depluralize(string name) =>
        name.EndsWith("ies") ? name[..^3] + "y"
        : name.EndsWith("s") && !name.EndsWith("ss") ? name[..^1]
        : name;
}
