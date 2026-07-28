namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Owns one selected baseline newobj caller and its routed template.</summary>
internal static class ProfiledConstructionFactoryCaller
{
    /// <summary>Executes the baseline newobj operation.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledConstructionFactoryTarget Selected(
        int value) =>
        new(value);

}
