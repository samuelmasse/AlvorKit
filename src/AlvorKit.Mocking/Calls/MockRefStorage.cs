namespace AlvorKit.Mocking;

/// <summary>Owns one stable setup-lifetime location used by value-based ref returns.</summary>
internal sealed class MockRefStorage<T>(T value)
{
    private T value = value;

    /// <summary>Returns the owned mutable location.</summary>
    internal ref T Mutable() => ref value;

    /// <summary>Returns the owned read-only location.</summary>
    internal ref readonly T ReadOnly() => ref value;
}
