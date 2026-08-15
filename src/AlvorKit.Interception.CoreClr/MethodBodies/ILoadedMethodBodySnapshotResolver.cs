namespace AlvorKit;

/// <summary>Resolves authoritative loaded-body snapshots for exact runtime method identities.</summary>
public interface ILoadedMethodBodySnapshotResolver
{
    /// <summary>Attempts to resolve the immutable loaded body for one exact method target.</summary>
    bool TryResolveLoadedBody(
        InterceptionTarget method,
        [NotNullWhen(true)] out LoadedMethodBodySnapshot? body);
}
