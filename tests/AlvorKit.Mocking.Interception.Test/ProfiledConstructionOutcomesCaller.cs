namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one selected baseline newobj caller and its outcome route.</summary>
internal static class ProfiledConstructionOutcomesCaller
{
    /// <summary>Executes the baseline newobj operation.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledConstructionOutcomesTarget Selected(
        int value) =>
        new(value);

}
