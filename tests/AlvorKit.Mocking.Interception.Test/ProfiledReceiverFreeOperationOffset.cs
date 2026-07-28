namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Finds one method, construction, or field opcode in its exact caller context.</summary>
internal static class ProfiledReceiverFreeOperationOffset
{
    private static readonly byte[] MemberOpcodes =
    [
        0x28,
        0x6F,
        0x73,
        0x7B,
        0x7D,
        0x7E,
        0x80,
    ];

    /// <summary>Finds the exact operation operand after resolving generic context.</summary>
    internal static int Find(
        MethodInfo caller,
        MemberInfo operation)
    {
        byte[] il = caller.GetMethodBody()?.GetILAsByteArray() ??
            throw new InvalidOperationException(
                "The receiver-free caller has no readable IL.");
        Type[] typeArguments =
            caller.DeclaringType?.GetGenericArguments() ?? [];
        Type[] methodArguments = caller.GetGenericArguments();
        for (var offset = 0; offset <= il.Length - 5; offset++)
        {
            if (!MemberOpcodes.Contains(il[offset]))
                continue;

            int token = BinaryPrimitives.ReadInt32LittleEndian(
                il.AsSpan(offset + 1));
            MemberInfo? resolved;
            try
            {
                resolved = caller.Module.ResolveMember(
                    token,
                    typeArguments,
                    methodArguments);
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (SameConstruction(resolved, operation))
                return offset;
        }

        throw new InvalidOperationException(
            "The receiver-free caller does not contain the expected " +
            $"operation '{operation}'.");
    }

    private static bool SameConstruction(
        MemberInfo? left,
        MemberInfo right)
    {
        if (left is null ||
            left.MetadataToken != right.MetadataToken ||
            left.DeclaringType != right.DeclaringType)
        {
            return false;
        }

        return left is not MethodInfo leftMethod ||
            right is not MethodInfo rightMethod ||
            leftMethod.GetGenericArguments().SequenceEqual(
                rightMethod.GetGenericArguments());
    }
}
