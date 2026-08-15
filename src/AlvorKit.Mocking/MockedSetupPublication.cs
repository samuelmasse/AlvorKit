namespace AlvorKit;

/// <summary>Materializes immutable argument patterns for configured behaviors.</summary>
internal static class MockedSetupPublication
{
    /// <summary>Adds a behavior and its immutable typed projector generation.</summary>
    internal static void Publish(
        Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockConfiguredBehavior behavior,
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        var patterns = new MockArgumentPattern[arguments.Length];
        for (var i = 0; i < arguments.Length; i++)
            patterns[i] = new(arguments[i]);

        mocked.AddSetup(
            new(
                method,
                patterns,
                behavior,
                projectors));
    }
}
