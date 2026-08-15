namespace AlvorKit;

/// <summary>Configures heap-safe values on receiver-free field clauses.</summary>
public static class MockReceiverFreeSetupClauseExtensions
{
    /// <summary>Returns one ordinary heap-safe value from a matching field read.</summary>
    public static void Return<T>(
        this MockFieldReadSetupClause<T> clause,
        T value)
    {
        ArgumentNullException.ThrowIfNull(clause);
        clause.AddOrdinaryReturn(value);
    }
}
