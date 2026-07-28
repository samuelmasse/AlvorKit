namespace AlvorKit.Mocking;

/// <summary>Adapts the public mutable delegate to the internal exact alias ABI.</summary>
internal sealed class MockRefReturnAdapter<T>(MockRefCall<T> factory)
{
    private readonly MockRefCall<T> factory = factory;

    /// <summary>Returns the configured mutable alias without retaining its value.</summary>
    internal ref T Invoke() => ref factory();
}

/// <summary>Adapts a read-only factory while the mocked signature preserves read-only use.</summary>
internal sealed class MockRefReadonlyReturnAdapter<T>(
    MockRefReadonlyCall<T> factory)
{
    private readonly MockRefReadonlyCall<T> factory = factory;

    /// <summary>Returns the stable alias through the internal mutable transport delegate.</summary>
    internal ref T Invoke()
    {
        ref readonly T value = ref factory();
        return ref System.Runtime.CompilerServices.Unsafe.AsRef(in value);
    }
}
