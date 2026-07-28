namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exposes the concrete addition wrapper as an exact handler.</summary>
public sealed class ProfiledAddHandler(ProfiledAddOperation wrapper)
{
    /// <summary>Invokes the bound Mocking addition wrapper.</summary>
    public int Invoke(
        ProfiledPartialTarget target,
        int left,
        int right) =>
        wrapper(target, left, right);
}

/// <summary>Exposes the concrete neighboring wrapper as an exact handler.</summary>
public sealed class ProfiledNeighborHandler(
    ProfiledNeighborOperation wrapper)
{
    /// <summary>Invokes the bound Mocking neighboring wrapper.</summary>
    public int Invoke(ProfiledPartialTarget target, int value) =>
        wrapper(target, value);
}

/// <summary>Exposes the concrete throwing wrapper as an exact handler.</summary>
public sealed class ProfiledThrowHandler(ProfiledThrowOperation wrapper)
{
    /// <summary>Invokes the bound Mocking throwing wrapper.</summary>
    public void Invoke(ProfiledPartialTarget target) =>
        wrapper(target);
}

/// <summary>Exposes the concrete ref/out wrapper as an exact handler.</summary>
public sealed class ProfiledMutateHandler(ProfiledMutateOperation wrapper)
{
    /// <summary>Invokes the bound Mocking ref/out wrapper.</summary>
    public int Invoke(
        ProfiledPartialTarget target,
        ref int value,
        out int doubled) =>
        wrapper(target, ref value, out doubled);
}
