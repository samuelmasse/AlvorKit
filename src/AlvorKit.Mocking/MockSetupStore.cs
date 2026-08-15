namespace AlvorKit;

/// <summary>
/// Publishes immutable newest-first setup generations for one mock.
/// </summary>
internal sealed class MockSetupStore
{
    private MockSetup[] setups = [];

    /// <summary>Adds a setup as the newest matching candidate.</summary>
    internal void Add(MockSetup setup)
    {
        lock (this)
        {
            var current = setups;
            var next = new MockSetup[current.Length + 1];
            next[0] = setup;
            current.CopyTo(next, 1);
            Volatile.Write(ref setups, next);
        }
    }

    /// <summary>
    /// Selects the newest setup matching the supplied method and arguments.
    /// Matcher user code runs without holding the setup-store lock.
    /// </summary>
    internal MockConfiguredBehavior? Find(
        MethodInfo method,
        ReadOnlySpan<object?> arguments,
        MockReceiverFreeIdentity? identity = null)
    {
        var snapshot = Volatile.Read(ref setups);

        foreach (var setup in snapshot)
        {
            if (setup.Matches(method, arguments, identity))
                return setup.Behavior;
        }

        return null;
    }

    /// <summary>Selects the newest matching setup including typed execution metadata.</summary>
    internal MockSetup? FindSetup(
        MethodInfo method,
        ReadOnlySpan<object?> arguments,
        MockReceiverFreeIdentity? identity = null)
    {
        MockSetup[] snapshot = Volatile.Read(ref setups);
        foreach (MockSetup setup in snapshot)
        {
            if (setup.Matches(method, arguments, identity))
                return setup;
        }

        return null;
    }

    /// <summary>Gets the current immutable setup generation.</summary>
    internal MockSetup[] Snapshot() => Volatile.Read(ref setups);

    /// <summary>Gets whether one method has any live typed matcher candidate.</summary>
    internal bool HasTypedMatchers(MethodInfo method)
    {
        MockSetup[] snapshot = Volatile.Read(ref setups);
        foreach (MockSetup setup in snapshot)
        {
            if (setup.Method == method &&
                setup.RequiresTypedEvaluation)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gets whether one method requires live typed execution.</summary>
    internal bool HasTypedExecution(MethodInfo method)
    {
        MockSetup[] snapshot = Volatile.Read(ref setups);
        foreach (MockSetup setup in snapshot)
        {
            if (setup.Method == method &&
                setup.RequiresTypedExecution)
            {
                return true;
            }
        }

        return false;
    }
}
