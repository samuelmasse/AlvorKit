namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Preserves the four untouched concrete operations used by wrapper fallbacks.</summary>
internal static class ProfiledPartialOriginal
{
    /// <summary>Invokes the untouched addition operation.</summary>
    internal static int Add(
        ProfiledPartialTarget target,
        int left,
        int right) =>
        target.Add(left, right);

    /// <summary>Invokes the untouched neighboring operation.</summary>
    internal static int Neighbor(
        ProfiledPartialTarget target,
        int value) =>
        target.Neighbor(value);

    /// <summary>Invokes the untouched throwing operation.</summary>
    internal static void Throw(ProfiledPartialTarget target) =>
        target.ThrowOriginal();

    /// <summary>Invokes the untouched ref/out operation.</summary>
    internal static int Mutate(
        ProfiledPartialTarget target,
        ref int value,
        out int doubled) =>
        target.Mutate(ref value, out doubled);
}
