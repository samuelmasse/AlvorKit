namespace AlvorKit.Mocking;

/// <summary>Configures heap-safe values on struct setup clauses.</summary>
public static class MockStructSetupClauseExtensions
{
    /// <summary>Returns one ordinary value copied into setup-owned storage.</summary>
    public static void Return<T, TResult>(
        this MockStructSetupClause<T, TResult> clause,
        TResult value)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(clause);
        clause.AddReturn(value);
    }
}
