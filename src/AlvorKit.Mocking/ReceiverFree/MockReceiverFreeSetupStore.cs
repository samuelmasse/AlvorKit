namespace AlvorKit.Mocking;

/// <summary>
/// Publishes immutable receiver-free setup generations with site-specific
/// precedence over member-wide setups.
/// </summary>
internal sealed class MockReceiverFreeSetupStore
{
    private MockReceiverFreeSetup[] setups = [];

    /// <summary>Adds one setup as the newest candidate in its scope.</summary>
    internal void Add(MockReceiverFreeSetup setup)
    {
        lock (this)
        {
            MockReceiverFreeSetup[] current = setups;
            var next =
                new MockReceiverFreeSetup[current.Length + 1];
            next[0] = setup;
            current.CopyTo(next, 1);
            Volatile.Write(ref setups, next);
        }
    }

    /// <summary>
    /// Selects the newest site-specific match before the newest member-wide
    /// match.
    /// </summary>
    internal MockReceiverFreeSetup? Find(
        MockReceiverFreeIdentity identity,
        ReadOnlySpan<object?> arguments)
    {
        MockReceiverFreeSetup[] snapshot =
            Volatile.Read(ref setups);
        foreach (MockReceiverFreeSetup setup in snapshot)
        {
            if (setup.Descriptor.Site is not null &&
                setup.Matches(identity, arguments))
            {
                return setup;
            }
        }

        foreach (MockReceiverFreeSetup setup in snapshot)
        {
            if (setup.Descriptor.Site is null &&
                setup.Matches(identity, arguments))
            {
                return setup;
            }
        }

        return null;
    }

    /// <summary>Gets the immutable current generation for typed evaluation.</summary>
    internal MockReceiverFreeSetup[] Snapshot() =>
        Volatile.Read(ref setups);
}
