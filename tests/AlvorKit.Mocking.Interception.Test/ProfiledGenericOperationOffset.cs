namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Finds a generic caller's exact constructed operation instruction.</summary>
internal static class ProfiledGenericOperationOffset
{
    /// <summary>Finds one call or callvirt operand after resolving its generic context.</summary>
    internal static int Find(
        MethodInfo caller,
        MethodInfo operation)
    {
        byte[] il = caller.GetMethodBody()?.GetILAsByteArray() ??
            throw new InvalidOperationException(
                "The selected caller has no readable IL.");
        Type[] typeArguments =
            caller.DeclaringType?.GetGenericArguments() ?? [];
        Type[] methodArguments = caller.GetGenericArguments();
        for (var offset = 0; offset <= il.Length - 5; offset++)
        {
            if (il[offset] is not (0x28 or 0x6F))
                continue;

            int token = BinaryPrimitives.ReadInt32LittleEndian(
                il.AsSpan(offset + 1));
            MethodBase? resolved;
            try
            {
                resolved = caller.Module.ResolveMethod(
                    token,
                    typeArguments,
                    methodArguments);
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (resolved is MethodInfo method &&
                SameConstruction(method, operation))
            {
                return offset;
            }
        }

        throw new InvalidOperationException(
            "The selected generic caller does not contain the expected " +
            $"operation '{operation}'.");
    }

    private static bool SameConstruction(
        MethodInfo left,
        MethodInfo right) =>
        left.MetadataToken == right.MetadataToken &&
        left.DeclaringType == right.DeclaringType &&
        left.GetGenericArguments().SequenceEqual(
            right.GetGenericArguments());
}
