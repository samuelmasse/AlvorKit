namespace AlvorKit.Mocking;

/// <summary>Owns one immutable, type-valid alternate value for an ordinary matcher type.</summary>
internal static class MockOrdinaryMatcherAlternate<T>
{
    /// <summary>Gets the immutable library-owned alternate capture value.</summary>
    internal static readonly T Value =
        (T)MockOrdinaryMatcherAlternate.Create(typeof(T))!;
}

/// <summary>Creates valid ordinary values without retaining caller-owned data.</summary>
internal static class MockOrdinaryMatcherAlternate
{
    /// <summary>Creates one value distinguishable from the default when the type permits it.</summary>
    internal static object? Create(Type type)
    {
        Type? nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType is not null)
        {
            object? underlying = Create(nullableType);
            return Activator.CreateInstance(type, underlying);
        }

        if (!type.IsValueType)
            return CreateReference(type);
        if (type.IsEnum)
            return Enum.ToObject(type, 1);
        if (type == typeof(bool))
            return true;
        if (type == typeof(char))
            return '\u0001';
        if (type == typeof(byte))
            return (byte)1;
        if (type == typeof(sbyte))
            return (sbyte)1;
        if (type == typeof(short))
            return (short)1;
        if (type == typeof(ushort))
            return (ushort)1;
        if (type == typeof(int))
            return 1;
        if (type == typeof(uint))
            return 1U;
        if (type == typeof(long))
            return 1L;
        if (type == typeof(ulong))
            return 1UL;
        if (type == typeof(nint))
            return (nint)1;
        if (type == typeof(nuint))
            return (nuint)1;
        if (type == typeof(Half))
            return (Half)1;
        if (type == typeof(float))
            return 1F;
        if (type == typeof(double))
            return 1D;
        if (type == typeof(decimal))
            return 1M;
        if (type == typeof(Int128))
            return (Int128)1;
        if (type == typeof(UInt128))
            return (UInt128)1;
        if (type == typeof(DateTime))
            return new DateTime(1);
        if (type == typeof(DateTimeOffset))
            return new DateTimeOffset(1, TimeSpan.Zero);
        if (type == typeof(TimeSpan))
            return TimeSpan.FromTicks(1);
        if (type == typeof(Guid))
            return new Guid(
                1,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);

        return CreateCompositeValue(type);
    }

    private static object CreateReference(Type type)
    {
        if (type == typeof(string))
            return "\u0001AlvorKit.Matcher";
        if (type.IsArray)
            return Array.CreateInstance(type.GetElementType()!, 0);
        if (typeof(Delegate).IsAssignableFrom(type))
        {
            throw new MockException(
                $"Unindexed ordinary matchers do not support delegate type '{type}'. " +
                "Use a declared-index matcher.");
        }

        Type concreteType =
            type.IsInterface || type.IsAbstract
                ? MockRuntimeBackendRegistry.Proxy.ResolveMockType(type)
                : type;
        return RuntimeHelpers.GetUninitializedObject(concreteType);
    }

    private static object CreateCompositeValue(Type type)
    {
        object value = Activator.CreateInstance(type)!;
        var comparer = new MockOrdinaryArgumentComparer();

        foreach (FieldInfo field in type.GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic))
        {
            object? alternate = Create(field.FieldType);
            field.SetValue(value, alternate);
            object baseline = Activator.CreateInstance(type)!;
            if (!comparer.Equals(value, baseline, type))
                return value;
        }

        return value;
    }
}
