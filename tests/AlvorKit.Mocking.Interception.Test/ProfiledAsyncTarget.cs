namespace AlvorKit;

/// <summary>Sealed target for the concrete asynchronous-answer behavior row.</summary>
public sealed class ProfiledAsyncTarget
{
    /// <summary>Gets the number of original asynchronous bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Returns asynchronously from the original implementation.</summary>
    public Task<int> AddAsync(int value)
    {
        OriginalCalls++;
        return Task.FromResult(value + 50);
    }
}

/// <summary>Exact delegate for the concrete asynchronous operation.</summary>
public delegate Task<int> ProfiledAsyncOperation(
    ProfiledAsyncTarget target,
    int value);

/// <summary>Preserves the untouched asynchronous operation for wrapper fallback.</summary>
internal static class ProfiledAsyncOriginal
{
    /// <summary>Invokes the untouched asynchronous operation.</summary>
    internal static Task<int> Invoke(
        ProfiledAsyncTarget target,
        int value) =>
        target.AddAsync(value);
}

/// <summary>Exposes the asynchronous Mocking wrapper as an exact handler.</summary>
public sealed class ProfiledAsyncHandler(ProfiledAsyncOperation wrapper)
{
    /// <summary>Invokes the bound asynchronous Mocking wrapper.</summary>
    public Task<int> Invoke(ProfiledAsyncTarget target, int value) =>
        wrapper(target, value);
}
