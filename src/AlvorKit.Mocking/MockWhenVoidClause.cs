namespace AlvorKit.Mocking;

/// <summary>Configures ref and out parameter behavior for a captured mocked void call.</summary>
public class MockWhenVoidClause
{
    /// <summary>The mock state owning the configured return.</summary>
    private readonly Mocked mocked;

    /// <summary>The captured method or accessor.</summary>
    private readonly MethodInfo method;

    /// <summary>The captured argument signature.</summary>
    private readonly object?[] args;

    /// <summary>Creates a return configuration clause for one captured void call.</summary>
    internal MockWhenVoidClause(Mocked mocked, MethodInfo method, object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
    }

    /// <summary>Configures optional ref or out parameter values for the captured call.</summary>
    public void Return(object?[] refs)
    {
        ReturnValues.Add(mocked, method, null, args, refs);
    }
}
