namespace AlvorKit.Mocking;

/// <summary>Holds one reusable exact wrapper method.</summary>
internal sealed class MockInterceptionWrapperArtifact(
    MockInterceptionWrapperCacheKey key,
    MethodInfo wrapper)
{
    /// <summary>Gets the immutable runtime metadata cache key.</summary>
    internal MockInterceptionWrapperCacheKey Key { get; } = key;

    /// <summary>Gets the emitted static method whose first argument is bound state.</summary>
    internal MethodInfo Wrapper { get; } = wrapper;
}
