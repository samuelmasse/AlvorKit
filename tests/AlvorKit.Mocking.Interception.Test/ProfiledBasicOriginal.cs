namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Preserves the untouched concrete operations used by wrapper fallbacks.</summary>
internal static class ProfiledBasicOriginal
{
    /// <summary>Invokes the untouched addition operation.</summary>
    internal static int Add(
        ProfiledBasicTarget target,
        int left,
        int right) =>
        target.Add(left, right);

    /// <summary>Invokes the untouched property getter.</summary>
    internal static int GetNumber(ProfiledBasicTarget target) =>
        target.Number;

    /// <summary>Invokes the untouched property setter.</summary>
    internal static void SetNumber(
        ProfiledBasicTarget target,
        int value) =>
        target.Number = value;

    /// <summary>Invokes the untouched ref/out operation.</summary>
    internal static int Mutate(
        ProfiledBasicTarget target,
        ref int value,
        out int doubled) =>
        target.Mutate(ref value, out doubled);

    /// <summary>Invokes the untouched event add accessor.</summary>
    internal static void AddChanged(
        ProfiledBasicTarget target,
        EventHandler? handler) =>
        target.Changed += handler;

    /// <summary>Invokes the untouched event remove accessor.</summary>
    internal static void RemoveChanged(
        ProfiledBasicTarget target,
        EventHandler? handler) =>
        target.Changed -= handler;
}
