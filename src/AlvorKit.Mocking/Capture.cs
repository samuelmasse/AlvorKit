namespace AlvorKit.Mocking;

/// <summary>Owns thread-local setup capture state for mocked invocations and argument matchers.</summary>
internal static class Capture
{
    /// <summary>The current capture state for this thread.</summary>
    private static readonly ThreadLocal<CaptureContext> context = new();

    /// <summary>Matchers captured during the first setup invocation.</summary>
    private static readonly ThreadLocal<List<Matcher>> firstMatchers = new(() => []);

    /// <summary>Matchers captured during the disambiguation invocation.</summary>
    private static readonly ThreadLocal<List<Matcher>> secondMatchers = new(() => []);

    /// <summary>Argument fingerprints captured during the first setup invocation.</summary>
    private static readonly ThreadLocal<ulong[]> firstFingerprints = new(() => new ulong[128]);

    /// <summary>Argument fingerprints captured during the disambiguation invocation.</summary>
    private static readonly ThreadLocal<ulong[]> secondFingerprints = new(() => new ulong[128]);

    /// <summary>Gets the current capture state for this thread.</summary>
    internal static CaptureContext Context => context.Value;

    /// <summary>Gets the first-pass matchers for this thread.</summary>
    internal static List<Matcher> FirstMatchers => firstMatchers.Value!;

    /// <summary>Gets the second-pass matchers for this thread.</summary>
    internal static List<Matcher> SecondMatchers => secondMatchers.Value!;

    /// <summary>Gets the first-pass argument fingerprints for this thread.</summary>
    internal static ulong[] FirstFingerprints => firstFingerprints.Value!;

    /// <summary>Gets the second-pass argument fingerprints for this thread.</summary>
    internal static ulong[] SecondFingerprints => secondFingerprints.Value!;

    /// <summary>Starts a setup or event capture operation for this thread.</summary>
    internal static void Start()
    {
        Array.Clear(FirstFingerprints);
        FirstMatchers.Clear();
        context.Value = new() { IsActive = true };
    }

    /// <summary>Switches the current setup capture into matcher disambiguation mode.</summary>
    internal static void Disambiguate()
    {
        Array.Clear(SecondFingerprints);
        SecondMatchers.Clear();
        context.Value = context.Value with { IsDisambiguating = true };
    }

    /// <summary>Ends capture and clears thread-local matcher lists.</summary>
    internal static void End()
    {
        context.Value = default;
        FirstMatchers.Clear();
        SecondMatchers.Clear();
    }

    /// <summary>Records a captured invocation and its argument values.</summary>
    internal static void Write(object cinstance, MethodInfo cmethod, object?[] cargs)
    {
        context.Value = context.Value with
        {
            Instance = cinstance,
            Method = cmethod,
            Args = cargs
        };
    }

    /// <summary>Records the invocation identity observed during matcher disambiguation.</summary>
    internal static void WriteDisambiguate(object cinstance, MethodInfo cmethod)
    {
        context.Value = context.Value with
        {
            Instance = cinstance,
            Method = cmethod,
        };
    }

    /// <summary>Stores an argument matcher in the active capture pass.</summary>
    internal static void WriteMatcher(Matcher matcher)
    {
        if (Context.IsDisambiguating)
            SecondMatchers.Add(matcher);
        else FirstMatchers.Add(matcher);
    }
}
