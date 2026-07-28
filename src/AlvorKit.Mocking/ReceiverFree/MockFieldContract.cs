namespace AlvorKit.Mocking;

/// <summary>Validates receiver requirements for typed field contracts.</summary>
internal static class MockFieldContract
{
    /// <summary>Validates an instance field and its exact receiver.</summary>
    internal static void ValidateInstance<TTarget, TValue>(
        TTarget target,
        MockField<TValue> field)
        where TTarget : class
        where TValue : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(field);
        if (field.IsStatic)
            throw new MockException($"Field '{field}' is static and has no receiver.");
        if (!field.Metadata.DeclaringType!.IsInstanceOfType(target))
        {
            throw new MockException(
                $"Receiver type '{target.GetType()}' is not assignable to field " +
                $"declaring type '{field.Metadata.DeclaringType}'.");
        }
    }

    /// <summary>Validates that a field contract describes static storage.</summary>
    internal static void ValidateStatic<TValue>(MockField<TValue> field)
        where TValue : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(field);
        if (!field.IsStatic)
            throw new MockException($"Field '{field}' requires an instance receiver.");
    }
}
