namespace AlvorKit;

/// <summary>Prepares concrete exact route methods used by generic caller constructions.</summary>
internal static class ProfiledGenericFunctionPointer
{
    /// <summary>Gets one exact nonpublic static managed function pointer.</summary>
    internal static nint Get(Type type, string name)
    {
        var method = type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}
