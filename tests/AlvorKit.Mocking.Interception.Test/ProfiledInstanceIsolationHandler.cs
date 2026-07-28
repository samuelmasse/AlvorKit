namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exposes the receiver-isolation wrapper as an exact handler.</summary>
public sealed class ProfiledInstanceIsolationHandler(
    ProfiledInstanceIsolationOperation wrapper)
{
    /// <summary>Invokes the bound Mocking addition wrapper.</summary>
    public int Invoke(
        ProfiledInstanceIsolationTarget target,
        int left,
        int right) =>
        wrapper(target, left, right);
}
