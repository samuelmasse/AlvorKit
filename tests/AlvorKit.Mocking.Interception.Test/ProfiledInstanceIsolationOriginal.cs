namespace AlvorKit;

/// <summary>Preserves the untouched receiver-isolation operation.</summary>
internal static class ProfiledInstanceIsolationOriginal
{
    /// <summary>Invokes the untouched addition operation.</summary>
    internal static int Add(
        ProfiledInstanceIsolationTarget target,
        int left,
        int right) =>
        target.Add(left, right);
}
