namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Owns one executable constructor generation and its extracted original remainder.</summary>
public sealed class LoadedConstructorRemainderGeneration
{
    /// <summary>Creates one complete constructor generation artifact.</summary>
    internal LoadedConstructorRemainderGeneration(
        InterceptionGenerationPlan plan,
        Delegate originalRemainder)
    {
        Plan = plan;
        OriginalRemainder = originalRemainder;
    }

    /// <summary>Gets the complete ABI-v3 constructor generation.</summary>
    public InterceptionGenerationPlan Plan { get; }

    /// <summary>Gets the exact receiver-and-arguments original remainder delegate.</summary>
    public Delegate OriginalRemainder { get; }
}
