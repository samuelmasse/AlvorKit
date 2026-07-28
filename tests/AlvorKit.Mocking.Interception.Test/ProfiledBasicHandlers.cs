namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exposes the concrete addition wrapper as an exact handler.</summary>
public sealed class ProfiledBasicAddHandler(
    ProfiledBasicAddOperation wrapper)
{
    /// <summary>Invokes the bound Mocking addition wrapper.</summary>
    public int Invoke(
        ProfiledBasicTarget target,
        int left,
        int right) =>
        wrapper(target, left, right);
}

/// <summary>Exposes the concrete property-getter wrapper as an exact handler.</summary>
public sealed class ProfiledBasicGetNumberHandler(
    ProfiledBasicGetNumberOperation wrapper)
{
    /// <summary>Invokes the bound Mocking property-getter wrapper.</summary>
    public int Invoke(ProfiledBasicTarget target) =>
        wrapper(target);
}

/// <summary>Exposes the concrete property-setter wrapper as an exact handler.</summary>
public sealed class ProfiledBasicSetNumberHandler(
    ProfiledBasicSetNumberOperation wrapper)
{
    /// <summary>Invokes the bound Mocking property-setter wrapper.</summary>
    public void Invoke(ProfiledBasicTarget target, int value) =>
        wrapper(target, value);
}

/// <summary>Exposes the concrete ref/out wrapper as an exact handler.</summary>
public sealed class ProfiledBasicMutateHandler(
    ProfiledBasicMutateOperation wrapper)
{
    /// <summary>Invokes the bound Mocking ref/out wrapper.</summary>
    public int Invoke(
        ProfiledBasicTarget target,
        ref int value,
        out int doubled) =>
        wrapper(target, ref value, out doubled);
}

/// <summary>Exposes one concrete event-accessor wrapper as an exact handler.</summary>
public sealed class ProfiledBasicEventHandler(
    ProfiledBasicEventOperation wrapper)
{
    /// <summary>Invokes the bound Mocking event-accessor wrapper.</summary>
    public void Invoke(
        ProfiledBasicTarget target,
        EventHandler? handler) =>
        wrapper(target, handler);
}
