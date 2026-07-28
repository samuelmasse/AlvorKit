namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one selected baseline newobj caller and validation route.</summary>
internal static class ProfiledConstructionValidationCaller
{
    /// <summary>Executes the baseline newobj operation.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledConstructionValidationTarget Selected(
        int value) =>
        new(value);

}
