namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Preserves exact original struct operations behind typed delegates.</summary>
internal static class ProfiledStructOriginal
{
    internal static int Add(
        ref ProfiledMutableStructTarget target,
        int amount) =>
        target.Add(amount);

    internal static int Read(
        in ProfiledReadonlyStructTarget target,
        int amount) =>
        target.Read(amount);

    internal static int ReadRecord(
        ref ProfiledRecordStructTarget target,
        int amount) =>
        target.Read(amount);

    internal static ProfiledStructWindow Window(
        ref ProfiledMutableStructTarget target,
        int[] owner) =>
        target.Window(owner);

    internal static int Constrained<T>(
        ref T target,
        int amount)
        where T : struct, IProfiledStructMetric =>
        target.Measure(amount);
}
