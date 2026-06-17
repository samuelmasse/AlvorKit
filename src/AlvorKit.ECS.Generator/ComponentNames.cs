namespace AlvorKit.ECS.Generator;

/// <summary>Contains naming rules for generated component source.</summary>
internal static class ComponentNames
{
    /// <summary>Removes the conventional interface prefix from generated component group type names.</summary>
    internal static string StripInterfacePrefix(string name) =>
        name.Length >= 2 && name[0] == 'I' && char.IsUpper(name[1])
            ? name.Substring(1)
            : name;

    /// <summary>Returns the accessor property name, avoiding direct delegate invocation ambiguity.</summary>
    internal static string AccessorName(PropertyModel property) =>
        property.IsDelegate ? property.Name + "Delegate" : property.Name;
}
