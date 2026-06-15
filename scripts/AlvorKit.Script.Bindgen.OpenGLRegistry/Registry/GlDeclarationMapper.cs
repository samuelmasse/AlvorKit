namespace AlvorKit.Script.Bindgen;

/// <summary>Maps OpenGL registry declarations into managed and raw interop shapes.</summary>
/// <param name="catchAllName">Generated catch-all enum name for untyped GLenum positions.</param>
internal sealed class GlDeclarationMapper(string catchAllName)
{
    /// <summary>Maps a registry declaration into the managed/raw pair used by commands.</summary>
    public GlDeclarationShape Map(
        XElement declaration,
        string commandName,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        IReadOnlyDictionary<string, string> callbackManagedNames,
        SortedSet<string> handleTypes,
        List<string> ungroupedEnumUses,
        HashSet<string> usedCallbacks,
        bool objectCommand = false)
    {
        var (name, cType) = ReadDeclaration(declaration);
        var pointerDepth = cType.Count(character => character == '*');
        var baseType = cType.Replace("const", "").Replace("struct", "").Replace("*", "").Trim();
        var group = declaration.Attribute("group")?.Value;
        var handleClass = declaration.Attribute("class")?.Value;
        if (pointerDepth == 0 && callbackManagedNames.TryGetValue(baseType, out var callbackManaged))
        {
            usedCallbacks.Add(baseType);
            return Shape(name, "nint", "nint", 0, callbackType: callbackManaged);
        }

        if (!GlRegistryValueTypes.Map.TryGetValue(baseType, out var valueType))
            throw new InvalidOperationException($"{commandName}: unmapped C type '{cType.Trim()}'.");

        if (pointerDepth > 0)
            return PointerShape(name, cType, baseType, valueType, group, handleClass, pointerDepth, managedNameByGroup, handleTypes);
        if (baseType is "GLenum" or "GLbitfield")
            return EnumShape(name, commandName, baseType, group, managedNameByGroup, ungroupedEnumUses);
        if (baseType == "GLint" && group is not null && managedNameByGroup.TryGetValue(group, out var intGroup))
            return Shape(name, intGroup, "int", 0);
        if (baseType == "GLuint" && GlRegistryHandleTypes.Resolve(handleClass, handleTypes) is { } handle)
            return Shape(name, handle, "uint", 0);
        if (baseType == "GLuint" && objectCommand)
        {
            handleTypes.Add("GlHandle");
            return Shape(name, "GlHandle", "uint", 0);
        }
        return baseType == "GLboolean" ? Shape(name, "bool", "byte", 0) : Shape(name, valueType, valueType, 0);
    }

    /// <summary>Reads native type text and name from a registry declaration element.</summary>
    private static (string Name, string CType) ReadDeclaration(XElement declaration)
    {
        var type = new StringBuilder();
        var name = "";
        foreach (var node in declaration.Nodes())
        {
            if (node is XElement { Name.LocalName: "name" } nameElement)
                name = nameElement.Value;
            else if (node is XElement element)
                type.Append(element.Value);
            else if (node is XText text)
                type.Append(text.Value);
        }
        return (name, type.ToString());
    }

    /// <summary>Builds a declaration shape for pointer declarations.</summary>
    private static GlDeclarationShape PointerShape(
        string name,
        string cType,
        string baseType,
        string valueType,
        string? group,
        string? handleClass,
        int pointerDepth,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        SortedSet<string> handleTypes)
    {
        var pointeeType = pointerDepth != 1 || valueType == "void" ? null
            : baseType == "GLenum" && group is not null && managedNameByGroup.TryGetValue(group, out var pointeeGroup) ? pointeeGroup
            : baseType == "GLuint" && GlRegistryHandleTypes.Resolve(handleClass, handleTypes) is { } pointeeHandle ? pointeeHandle
            : valueType;
        return Shape(name, "nint", "nint", pointerDepth, pointeeType, cType.TrimStart().StartsWith("const "), baseType == "GLchar");
    }

    /// <summary>Builds a declaration shape for scalar GLenum and GLbitfield declarations.</summary>
    private GlDeclarationShape EnumShape(
        string name,
        string commandName,
        string baseType,
        string? group,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        List<string> ungroupedEnumUses)
    {
        if (group is not null && managedNameByGroup.TryGetValue(group, out var managedGroup))
            return Shape(name, managedGroup, "uint", 0);

        ungroupedEnumUses.Add($"{commandName}({(name.Length > 0 ? name : "return")}: {group ?? "no group"})");
        return Shape(name, baseType == "GLenum" ? catchAllName : "uint", "uint", 0);
    }

    /// <summary>Creates a declaration shape with common defaults.</summary>
    private static GlDeclarationShape Shape(
        string name,
        string managed,
        string interop,
        int pointerDepth,
        string? pointeeType = null,
        bool pointeeIsConst = false,
        bool pointeeIsChar = false,
        string? callbackType = null) =>
        new(name, new(managed, interop), pointerDepth, pointeeType, pointeeIsConst, pointeeIsChar, callbackType);
}
