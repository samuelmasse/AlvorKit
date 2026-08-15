namespace AlvorKit;

/// <summary>Interprets standard state-machine metadata without reading reflection bodies.</summary>
internal static class LoadedSourceMethodMetadata
{
    /// <summary>Gets a supported exact state-machine marker kind.</summary>
    internal static bool TryKind(
        Type attributeType,
        out LoadedSourceMethodKind kind)
    {
        if (attributeType == typeof(AsyncStateMachineAttribute))
            kind = LoadedSourceMethodKind.Async;
        else if (attributeType == typeof(IteratorStateMachineAttribute))
            kind = LoadedSourceMethodKind.Iterator;
        else if (attributeType == typeof(AsyncIteratorStateMachineAttribute))
            kind = LoadedSourceMethodKind.AsyncIterator;
        else
        {
            kind = default;
            return false;
        }

        return true;
    }

    /// <summary>Reads the exact generated type from one standard marker argument.</summary>
    internal static bool TryStateMachineType(
        CustomAttributeData attribute,
        out Type stateMachineType)
    {
        if (attribute.ConstructorArguments.Count == 1 &&
            attribute.ConstructorArguments[0].ArgumentType == typeof(Type) &&
            attribute.ConstructorArguments[0].Value is Type value)
        {
            stateMachineType = value;
            return true;
        }

        stateMachineType = null!;
        return false;
    }

    /// <summary>Gets whether one declared method has the exact generated MoveNext shape.</summary>
    internal static bool IsMoveNext(
        MethodInfo method,
        LoadedSourceMethodKind kind) =>
        method.Name == "MoveNext" &&
        !method.IsStatic &&
        !method.IsGenericMethodDefinition &&
        method.ReturnType ==
            (kind == LoadedSourceMethodKind.Iterator
                ? typeof(bool)
                : typeof(void)) &&
        method.GetParameters().Length == 0;

    /// <summary>Gets a MethodDef token without allowing diagnostics to throw.</summary>
    internal static int TokenOrZero(MethodInfo method)
    {
        try
        {
            return method.MetadataToken;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    /// <summary>Formats one deterministic source or generated method attribution.</summary>
    internal static string Display(MethodInfo method) =>
        $"{TypeName(method.DeclaringType)}::{method.Name} " +
        $"[0x{TokenOrZero(method):X8}]";

    /// <summary>Formats one deterministic type name.</summary>
    internal static string TypeName(Type? type) =>
        type?.FullName ?? type?.Name ?? "<global>";
}
