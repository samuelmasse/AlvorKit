namespace AlvorKit.Script.Bindgen;

/// <summary>Formats generated method signatures and call arguments.</summary>
internal static class BindingSignature
{
    /// <summary>Formats a generated function signature.</summary>
    public static string ForFunction(BindingFunction function, bool native = false) =>
        string.Join(", ", function.Parameters.Select(parameter =>
        {
            var modifier = parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "";
            return $"{modifier}{(native ? parameter.InteropType : parameter.ManagedType)} {parameter.ManagedName}";
        }));

    /// <summary>Formats one managed signature parameter.</summary>
    public static string Parameter(BindingParameter parameter) =>
        $"{(parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "")}{parameter.ManagedType} {parameter.ManagedName}";

    /// <summary>Formats parameter types for XML documentation cref signatures.</summary>
    public static string Cref(IEnumerable<BindingParameter> parameters) =>
        string.Join(", ", parameters.Select(parameter => (parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "") + parameter.ManagedType));

    /// <summary>Formats an argument for forwarding between managed layers.</summary>
    public static string Argument(BindingParameter parameter) =>
        (parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "") + parameter.ManagedName;

    /// <summary>Formats an argument for a native call, including managed-to-interop casts.</summary>
    public static string NativeArgument(BindingParameter parameter)
    {
        var modifier = parameter.Modifier.Length > 0 ? parameter.Modifier + " " : "";
        if (parameter.ManagedType == parameter.InteropType)
            return modifier + parameter.ManagedName;
        if (parameter.ManagedType == "bool")
            return $"{parameter.ManagedName} ? ({parameter.InteropType})1 : ({parameter.InteropType})0";
        if (parameter.Modifier.Length == 0 && parameter.ManagedType == "UInt128" && parameter.InteropType != "UInt128")
            return $"{parameter.InteropType}.FromUInt128({parameter.ManagedName})";
        return $"{modifier}({parameter.InteropType}){parameter.ManagedName}";
    }

    /// <summary>Returns the C# unsafe modifier when a function signature uses pointer types.</summary>
    public static string UnsafeModifier(BindingFunction function) =>
        function.ReturnType.Contains('*') || function.ReturnInteropType.Contains('*') || function.Parameters.Any(RequiresUnsafe)
            ? "unsafe "
            : "";

    /// <summary>Formats a type parameter suffix from a managed parameter name.</summary>
    public static string TypeParameterName(string managedParameterName)
    {
        var name = managedParameterName.TrimStart('@');
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    /// <summary>Returns true when a parameter exposes a pointer type in managed or native signatures.</summary>
    private static bool RequiresUnsafe(BindingParameter parameter) =>
        parameter.ManagedType.Contains('*') || parameter.InteropType.Contains('*');
}
