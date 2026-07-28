namespace AlvorKit.Mocking;

/// <summary>
/// Compares capture-pass ordinary values without invoking user equality or
/// hash code implementations.
/// </summary>
internal sealed class MockOrdinaryArgumentComparer
{
    private readonly HashSet<(object First, object Second)> visited =
        new(MockReferencePairComparer.Instance);

    /// <summary>Returns whether two capture-pass values have the same safe shape.</summary>
    internal bool Equals(object? first, object? second, Type declaredType)
    {
        if (first is null || second is null)
            return first is null && second is null;

        Type valueType = Nullable.GetUnderlyingType(declaredType) ??
            declaredType;
        if (!valueType.IsValueType)
            return EqualsReference(first, second, valueType);
        if (IsBuiltInValue(valueType))
            return first.Equals(second);
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new MockException(
                $"Generated mocking cannot structurally compare ordinary " +
                $"value type '{valueType}' without generated field metadata. " +
                "Use a declared-index matcher for this argument.");
        }

        foreach (FieldInfo field in valueType.GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic))
        {
            if (!Equals(
                field.GetValue(first),
                field.GetValue(second),
                field.FieldType))
            {
                return false;
            }
        }

        return true;
    }

    private bool EqualsReference(
        object first,
        object second,
        Type declaredType)
    {
        if (ReferenceEquals(first, second))
            return true;
        if (declaredType == typeof(string))
        {
            return ((string)first).AsSpan().SequenceEqual(
                ((string)second).AsSpan());
        }

        if (!declaredType.IsArray ||
            first is not Array firstArray ||
            second is not Array secondArray ||
            firstArray.Rank != 1 ||
            secondArray.Rank != 1 ||
            firstArray.Length != secondArray.Length)
        {
            return false;
        }

        if (!visited.Add((first, second)))
            return true;

        Type elementType = declaredType.GetElementType()!;
        for (var index = 0; index < firstArray.Length; index++)
        {
            if (!Equals(
                firstArray.GetValue(index),
                secondArray.GetValue(index),
                elementType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsBuiltInValue(Type type) =>
        type.IsPrimitive ||
        type.IsEnum ||
        type == typeof(decimal) ||
        type == typeof(DateTime) ||
        type == typeof(DateTimeOffset) ||
        type == typeof(Guid) ||
        type == typeof(TimeSpan) ||
        type == typeof(Int128) ||
        type == typeof(UInt128);
}
