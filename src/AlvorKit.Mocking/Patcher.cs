namespace AlvorKit.Mocking;

/// <summary>Coordinates Harmony patches for mockable concrete methods.</summary>
internal static partial class Patcher
{
    /// <summary>Harmony instance used to attach runtime prefixes to target methods.</summary>
    private static readonly Harmony harmony = new(typeof(Patcher).FullName);

    /// <summary>Concrete types that have already been scanned and patched.</summary>
    private static readonly ConcurrentDictionary<Type, bool> patched = [];

    /// <summary>Concrete methods that already have a Harmony prefix installed.</summary>
    private static readonly ConcurrentDictionary<MethodInfo, bool> patchedMethods = [];

    /// <summary>Serializes patch installation because Harmony and dynamic-method caches are shared process state.</summary>
    private static readonly Lock patchLock = new();

    /// <summary>Prefix dynamic methods keyed by the original method they wrap.</summary>
    private static readonly Dictionary<MethodBase, DynamicMethod> dynamicMethods = [];

    /// <summary>Patches every supported instance method declared by a type and its base types.</summary>
    internal static void Patch(Type type)
    {
        if (type == typeof(object))
            return;

        if (patched.ContainsKey(type))
            return;

        lock (patchLock)
        {
            if (patched.ContainsKey(type))
                return;

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.DeclaringType != type && method.DeclaringType != null)
                {
                    Patch(method.DeclaringType);
                    continue;
                }

                if (method.Name.Contains(nameof(IMock.__Mocked_cc6d2cf7)))
                    continue;

                if (method.IsAbstract || method.ContainsGenericParameters)
                    continue;

                if (ReturnsRefStruct(method))
                    continue;

                PatchMethod(method);
            }

            patched.TryAdd(type, true);
        }
    }

    /// <summary>Patches one concrete method if it has not already been patched.</summary>
    internal static void PatchMethod(MethodInfo method)
    {
        if (patchedMethods.ContainsKey(method))
            return;

        lock (patchLock)
        {
            if (patchedMethods.ContainsKey(method))
                return;

            if (ReturnsRefStruct(method))
                throw new MockException($"Cannot patch method {method.Name} because it returns a ref struct or a ref to a ref struct.");

            harmony.Patch(
                method,
                prefix: typeof(Patcher).GetMethod(nameof(DynamicMethod), BindingFlags.Static | BindingFlags.NonPublic));

            patchedMethods.TryAdd(method, true);
        }
    }

    /// <summary>Returns true when a method returns a ref struct directly or by reference.</summary>
    private static bool ReturnsRefStruct(MethodInfo method) =>
        method.ReturnType.IsByRefLike || (method.ReturnType.IsByRef && method.ReturnType.GetElementType()!.IsByRefLike);
}
