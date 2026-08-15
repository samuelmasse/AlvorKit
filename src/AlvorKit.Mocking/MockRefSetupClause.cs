namespace AlvorKit;

/// <summary>Configures a captured mutable managed-reference return.</summary>
public sealed class MockRefSetupClause<T>
{
    private readonly Mocked mocked;
    private readonly MethodInfo method;
    private readonly object?[] args;

    /// <summary>Creates a mutable managed-reference setup clause.</summary>
    internal MockRefSetupClause(
        Mocked mocked,
        MethodInfo method,
        object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
    }

    /// <summary>Returns the exact stable reference produced by the factory.</summary>
    public void ReturnRef(MockRefCall<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        mocked.AddRefReturnFactory(method, args, factory);
    }

    /// <summary>Returns a reference to stable storage owned by this setup.</summary>
    public void ReturnRef(T value)
    {
        var storage = new MockRefStorage<T>(value);
        ReturnRef(storage.Mutable);
    }

    /// <summary>Configures the captured call to throw the supplied exception.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        mocked.AddThrow(method, args, exception);
    }
}
