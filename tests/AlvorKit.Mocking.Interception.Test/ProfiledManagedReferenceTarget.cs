namespace AlvorKit;

/// <summary>Sealed target with mutable and readonly managed-reference returns.</summary>
public sealed class ProfiledManagedReferenceTarget(int[]? aliases = null)
{
    private readonly int[] aliases = aliases ?? [13, 21];

    /// <summary>Gets the number of original managed-reference bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Gets the original alias storage.</summary>
    public int[] AliasStorage => aliases;

    /// <summary>Returns a mutable alias to the first original value.</summary>
    public ref int Mutable()
    {
        OriginalCalls++;
        return ref aliases[0];
    }

    /// <summary>Returns a readonly alias to the second original value.</summary>
    public ref readonly int ReadOnly()
    {
        OriginalCalls++;
        return ref aliases[1];
    }
}

/// <summary>Owns configured mutable and readonly aliases.</summary>
internal sealed class ProfiledAliasOwner(int[] values)
{
    /// <summary>Returns the configured mutable alias.</summary>
    internal ref int Mutable() => ref values[0];

    /// <summary>Returns the configured readonly alias.</summary>
    internal ref readonly int ReadOnly() => ref values[1];
}

/// <summary>Exact delegate for a mutable managed-reference return.</summary>
public delegate ref int ProfiledMutableReferenceOperation(
    ProfiledManagedReferenceTarget target);

/// <summary>Exact delegate for a readonly managed-reference return.</summary>
public delegate ref readonly int ProfiledReadOnlyReferenceOperation(
    ProfiledManagedReferenceTarget target);

/// <summary>Preserves untouched managed-reference operations for fallback.</summary>
internal static class ProfiledManagedReferenceOriginal
{
    /// <summary>Invokes the untouched mutable operation.</summary>
    internal static ref int Mutable(
        ProfiledManagedReferenceTarget target) =>
        ref target.Mutable();

    /// <summary>Invokes the untouched readonly operation.</summary>
    internal static ref readonly int ReadOnly(
        ProfiledManagedReferenceTarget target) =>
        ref target.ReadOnly();
}

/// <summary>Exposes the mutable managed-reference wrapper as an exact handler.</summary>
public sealed class ProfiledMutableReferenceHandler(
    ProfiledMutableReferenceOperation wrapper)
{
    /// <summary>Invokes the bound mutable managed-reference wrapper.</summary>
    public ref int Invoke(ProfiledManagedReferenceTarget target) =>
        ref wrapper(target);
}

/// <summary>Exposes the readonly managed-reference wrapper as an exact handler.</summary>
public sealed class ProfiledReadOnlyReferenceHandler(
    ProfiledReadOnlyReferenceOperation wrapper)
{
    /// <summary>Invokes the bound readonly managed-reference wrapper.</summary>
    public ref readonly int Invoke(
        ProfiledManagedReferenceTarget target) =>
        ref wrapper(target);
}
