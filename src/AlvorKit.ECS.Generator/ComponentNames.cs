namespace AlvorKit.ECS.Generator;

internal static class ComponentNames
{
        internal static string StripInterfacePrefix(string name) =>
        name.Length >= 2 && name[0] == 'I' && char.IsUpper(name[1])
            ? name.Substring(1)
            : name;

        internal static string AccessorName(PropertyModel property) =>
        property.IsDelegate ? property.Name + "Delegate" : property.Name;
}
