namespace AlvorKit;

/// <summary>Exposes one closed generic target echo wrapper as an exact handler.</summary>
public sealed class ProfiledClosedGenericEchoHandler<T>(
    ProfiledClosedGenericEchoOperation<T> wrapper)
{
    /// <summary>Invokes the bound Mocking echo wrapper.</summary>
    public T Invoke(ProfiledGenericTarget<T> target, T value) =>
        wrapper(target, value);
}

/// <summary>Exposes one closed generic property-getter wrapper as an exact handler.</summary>
public sealed class ProfiledClosedGenericValueHandler<T>(
    ProfiledClosedGenericValueOperation<T> wrapper)
{
    /// <summary>Invokes the bound Mocking property-getter wrapper.</summary>
    public T Invoke(ProfiledGenericTarget<T> target) =>
        wrapper(target);
}

/// <summary>Exposes one constructed generic method wrapper as an exact handler.</summary>
public sealed class ProfiledConstructedGenericEchoHandler<T>(
    ProfiledConstructedGenericEchoOperation<T> wrapper)
{
    /// <summary>Invokes the bound Mocking constructed-method wrapper.</summary>
    public T Invoke(
        ProfiledConstructedGenericTarget target,
        T value) =>
        wrapper(target, value);
}
